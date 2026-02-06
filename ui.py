import json
import os

from PyQt5.QtCore import QSettings, Qt, QThread, pyqtSignal
from PyQt5.QtGui import QCloseEvent, QFont, QIcon
from PyQt5.QtWidgets import (
    QCheckBox,
    QComboBox,
    QFileDialog,
    QGroupBox,
    QHBoxLayout,
    QHeaderView,
    QLabel,
    QLineEdit,
    QMainWindow,
    QMessageBox,
    QProgressBar,
    QPushButton,
    QSlider,
    QSpinBox,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
    QWidget,
)

from compressor import ImageCompressor

try:
    from qfluentwidgets import setThemeColor

    HAS_FLUENT_THEME = True
except Exception:
    HAS_FLUENT_THEME = False


class CompressionThread(QThread):
    progress_signal = pyqtSignal(int, int, dict)
    finished_signal = pyqtSignal(list, dict)

    def __init__(self, directory=None, files=None, output_format='original', quality=85, rename_settings=None):
        super().__init__()
        self.directory = directory
        self.files = files
        self.output_format = output_format
        self.quality = quality
        self.rename_settings = rename_settings
        self.compressor = ImageCompressor()

    def run(self):
        def progress_callback(processed, total, result):
            if self.rename_settings and self.rename_settings['enabled'] and result and result['success']:
                original_output_path = result['compressed_path']
                dir_path = os.path.dirname(original_output_path)
                ext = os.path.splitext(original_output_path)[1]

                prefix = self.rename_settings['prefix']
                separator = self.rename_settings['separator']
                number = self.rename_settings['start_number'] + processed - 1

                digits = len(str(total + self.rename_settings['start_number'] - 1))
                formatted_number = str(number).zfill(digits)
                new_filename = f"{prefix}{separator}{formatted_number}{ext}"
                new_path = os.path.join(dir_path, new_filename)

                try:
                    os.rename(original_output_path, new_path)
                    result['compressed_path'] = new_path
                except Exception as error:
                    print(f"Error renaming file: {error}")

            self.progress_signal.emit(processed, total, result)

        if self.directory:
            results, summary = self.compressor.compress_directory(
                self.directory,
                output_format=self.output_format,
                quality=self.quality,
                callback=progress_callback,
            )
        else:
            results = []
            total_original_size = 0
            total_compressed_size = 0
            total_files = len(self.files)

            for index, file_path in enumerate(self.files):
                result = self.compressor.compress_image(
                    file_path,
                    output_format=self.output_format,
                    quality=self.quality,
                )

                if result and result['success']:
                    results.append(result)
                    total_original_size += result['original_size']
                    total_compressed_size += result['compressed_size']

                progress_callback(index + 1, total_files, result)

            overall_ratio = (1 - total_compressed_size / total_original_size) * 100 if total_original_size > 0 else 0
            summary = {
                'total_files': total_files,
                'processed_files': len(results),
                'total_original_size': total_original_size,
                'total_compressed_size': total_compressed_size,
                'total_original_size_formatted': self.compressor.get_formatted_size(total_original_size),
                'total_compressed_size_formatted': self.compressor.get_formatted_size(total_compressed_size),
                'overall_compression_ratio': overall_ratio,
            }

        self.finished_signal.emit(results, summary)


class ImageCompressorUI(QMainWindow):
    def __init__(self):
        super().__init__()
        self.compressor = ImageCompressor()
        self.settings = QSettings('img-yasuo', 'ImageCompressorClassicUI')

        self.selected_directory = None
        self.selected_files = []

        self.init_ui()
        self.load_settings()

    def init_ui(self):
        self.setFont(QFont('Microsoft YaHei', 9))
        self.setWindowIcon(QIcon('icon.ico'))
        self.setWindowTitle('图片压缩工具')
        self.setGeometry(100, 100, 860, 640)

        if HAS_FLUENT_THEME:
            setThemeColor('#0078D4')

        self.setStyleSheet(
            """
            QMainWindow { background: #f7f8fa; }
            QGroupBox {
                font-weight: 600;
                border: 1px solid #d9dce3;
                border-radius: 10px;
                margin-top: 10px;
                padding-top: 10px;
                background: #ffffff;
            }
            QGroupBox::title { left: 10px; padding: 0 4px; }
            QPushButton {
                background: #ffffff;
                border: 1px solid #cdd3de;
                border-radius: 8px;
                padding: 6px 12px;
            }
            QPushButton:hover { background: #f3f6fb; border: 1px solid #95b6e8; }
            QPushButton:disabled { color: #9ca3af; background: #f3f4f6; }
            QProgressBar {
                border: 1px solid #d1d5db;
                border-radius: 7px;
                text-align: center;
                background: #eef2f7;
            }
            QProgressBar::chunk { background-color: #1677ff; border-radius: 6px; }
            QTableWidget {
                border: 1px solid #d9dce3;
                border-radius: 10px;
                background: #ffffff;
                gridline-color: #eef0f4;
            }
            QHeaderView::section {
                background: #f5f7fb;
                border: none;
                border-bottom: 1px solid #e4e8ef;
                padding: 6px;
                font-weight: 600;
            }
            """
        )

        main_widget = QWidget()
        main_layout = QVBoxLayout(main_widget)
        main_layout.setSpacing(10)

        dir_group = QGroupBox('选择文件')
        dir_layout = QVBoxLayout(dir_group)

        dir_row = QHBoxLayout()
        self.dir_label = QLabel('未选择目录')
        self.dir_button = QPushButton('选择目录')
        self.dir_button.clicked.connect(self.select_directory)
        dir_row.addWidget(self.dir_label)
        dir_row.addWidget(self.dir_button)

        file_row = QHBoxLayout()
        self.file_label = QLabel('未选择文件')
        self.file_button = QPushButton('选择文件')
        self.file_button.clicked.connect(self.select_files)
        file_row.addWidget(self.file_label)
        file_row.addWidget(self.file_button)

        dir_layout.addLayout(dir_row)
        dir_layout.addLayout(file_row)

        settings_group = QGroupBox('压缩设置')
        settings_layout = QVBoxLayout(settings_group)

        format_row = QHBoxLayout()
        format_row.addWidget(QLabel('输出格式:'))
        self.format_combo = QComboBox()
        self.format_combo.addItem('保持原格式', 'original')
        self.format_combo.addItem('JPEG', 'jpeg')
        self.format_combo.addItem('PNG', 'png')
        self.format_combo.addItem('WebP', 'webp')
        self.format_combo.currentIndexChanged.connect(self.toggle_quality_settings)
        format_row.addWidget(self.format_combo)
        format_row.addStretch(1)

        quality_row = QHBoxLayout()
        quality_row.addWidget(QLabel('JPEG质量:'))
        self.quality_slider = QSlider(Qt.Horizontal)
        self.quality_slider.setRange(1, 100)
        self.quality_slider.setValue(85)
        self.quality_slider.setTickPosition(QSlider.TicksBelow)
        self.quality_slider.setTickInterval(10)

        self.quality_value = QSpinBox()
        self.quality_value.setRange(1, 100)
        self.quality_value.setValue(85)

        self.quality_slider.valueChanged.connect(self.quality_value.setValue)
        self.quality_value.valueChanged.connect(self.quality_slider.setValue)

        quality_row.addWidget(self.quality_slider)
        quality_row.addWidget(self.quality_value)

        settings_layout.addLayout(format_row)
        settings_layout.addLayout(quality_row)

        rename_group = QGroupBox('重命名设置')
        rename_layout = QVBoxLayout(rename_group)

        self.rename_checkbox = QCheckBox('启用重命名')
        self.rename_checkbox.toggled.connect(self.toggle_rename_settings)

        prefix_row = QHBoxLayout()
        prefix_row.addWidget(QLabel('前缀:'))
        self.prefix_input = QLineEdit()
        self.prefix_input.setPlaceholderText('输入文件名前缀')
        self.prefix_input.setEnabled(False)
        prefix_row.addWidget(self.prefix_input)

        sep_row = QHBoxLayout()
        sep_row.addWidget(QLabel('分隔符:'))
        self.separator_combo = QComboBox()
        self.separator_combo.addItem('-', '-')
        self.separator_combo.addItem('_', '_')
        self.separator_combo.setEnabled(False)
        sep_row.addWidget(self.separator_combo)
        sep_row.addStretch(1)

        num_row = QHBoxLayout()
        num_row.addWidget(QLabel('起始编号:'))
        self.number_spinbox = QSpinBox()
        self.number_spinbox.setRange(1, 9999)
        self.number_spinbox.setValue(1)
        self.number_spinbox.setEnabled(False)
        num_row.addWidget(self.number_spinbox)
        num_row.addStretch(1)

        rename_layout.addWidget(self.rename_checkbox)
        rename_layout.addLayout(prefix_row)
        rename_layout.addLayout(sep_row)
        rename_layout.addLayout(num_row)

        self.compress_button = QPushButton('开始压缩')
        self.compress_button.setMinimumHeight(48)
        self.compress_button.setStyleSheet('font-size: 14px; font-weight: 700;')
        self.compress_button.clicked.connect(self.start_compression)
        self.compress_button.setEnabled(False)

        self.progress_bar = QProgressBar()
        self.progress_bar.setVisible(False)

        self.results_table = QTableWidget(0, 4)
        self.results_table.setHorizontalHeaderLabels(['文件名', '原始大小', '压缩后大小', '压缩比例'])
        self.results_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.Stretch)

        summary_group = QGroupBox('压缩结果汇总')
        summary_layout = QVBoxLayout(summary_group)
        self.total_files_label = QLabel('总文件数: 0')
        self.total_original_size_label = QLabel('压缩前总大小: 0 B')
        self.total_compressed_size_label = QLabel('压缩后总大小: 0 B')
        self.overall_ratio_label = QLabel('总体压缩比例: 0%')

        capabilities = self.compressor.get_capabilities()
        self.engine_label = QLabel(
            f"压缩引擎: PNG={capabilities['png_quantization']} | 抖动={capabilities['png_dithering']} | "
            f"无损={capabilities['png_lossless']} | JPEG={capabilities['jpeg_optimizer']}"
        )

        summary_layout.addWidget(self.total_files_label)
        summary_layout.addWidget(self.total_original_size_label)
        summary_layout.addWidget(self.total_compressed_size_label)
        summary_layout.addWidget(self.overall_ratio_label)
        summary_layout.addWidget(self.engine_label)

        main_layout.addWidget(dir_group)
        main_layout.addWidget(settings_group)
        main_layout.addWidget(rename_group)
        main_layout.addWidget(self.compress_button)
        main_layout.addWidget(self.progress_bar)
        main_layout.addWidget(self.results_table)
        main_layout.addWidget(summary_group)

        self.setCentralWidget(main_widget)
        self.toggle_quality_settings(self.format_combo.currentIndex())

    def _as_bool(self, value, default=False):
        if value is None:
            return default
        if isinstance(value, bool):
            return value
        return str(value).lower() in ['1', 'true', 'yes', 'on']

    def load_settings(self):
        geometry = self.settings.value('window/geometry')
        if geometry:
            self.restoreGeometry(geometry)

        self.quality_value.setValue(int(self.settings.value('compress/quality', 85)))
        saved_format = self.settings.value('compress/output_format', 'original')
        idx = self.format_combo.findData(saved_format)
        self.format_combo.setCurrentIndex(idx if idx >= 0 else 0)

        rename_enabled = self._as_bool(self.settings.value('rename/enabled', False))
        self.rename_checkbox.setChecked(rename_enabled)
        self.prefix_input.setText(self.settings.value('rename/prefix', ''))

        saved_separator = self.settings.value('rename/separator', '-')
        sep_idx = self.separator_combo.findData(saved_separator)
        self.separator_combo.setCurrentIndex(sep_idx if sep_idx >= 0 else 0)
        self.number_spinbox.setValue(int(self.settings.value('rename/start_number', 1)))

        saved_dir = self.settings.value('select/last_directory', '')
        if saved_dir and os.path.isdir(saved_dir):
            self.selected_directory = saved_dir

        files_text = self.settings.value('select/last_files', '[]')
        try:
            saved_files = json.loads(files_text)
            self.selected_files = [f for f in saved_files if os.path.isfile(f)]
        except Exception:
            self.selected_files = []

        last_mode = self.settings.value('select/last_mode', '')
        if last_mode == 'files' and self.selected_files:
            self.dir_label.setText('未选择目录')
            self.file_label.setText(os.path.basename(self.selected_files[0]) if len(self.selected_files) == 1 else f'已选择 {len(self.selected_files)} 个文件')
            self.selected_directory = None
            self.compress_button.setEnabled(True)
        elif last_mode == 'directory' and self.selected_directory:
            self.dir_label.setText(self.selected_directory)
            self.file_label.setText('未选择文件')
            self.selected_files = []
            self.compress_button.setEnabled(True)

    def save_settings(self):
        self.settings.setValue('window/geometry', self.saveGeometry())
        self.settings.setValue('compress/quality', self.quality_value.value())
        self.settings.setValue('compress/output_format', self.format_combo.currentData())

        self.settings.setValue('rename/enabled', self.rename_checkbox.isChecked())
        self.settings.setValue('rename/prefix', self.prefix_input.text())
        self.settings.setValue('rename/separator', self.separator_combo.currentData())
        self.settings.setValue('rename/start_number', self.number_spinbox.value())

        if self.selected_directory:
            self.settings.setValue('select/last_directory', self.selected_directory)
        if self.selected_files:
            self.settings.setValue('select/last_files', json.dumps(self.selected_files, ensure_ascii=False))

        self.settings.setValue('select/last_mode', 'files' if self.selected_files else ('directory' if self.selected_directory else ''))
        self.settings.sync()

    def select_directory(self):
        base_dir = self.selected_directory if self.selected_directory else os.path.expanduser('~')
        directory = QFileDialog.getExistingDirectory(self, '选择目录', base_dir)
        if directory:
            self.selected_directory = directory
            self.selected_files = []
            self.dir_label.setText(directory)
            self.file_label.setText('未选择文件')
            self.compress_button.setEnabled(True)
            self.save_settings()

    def select_files(self):
        base_dir = self.selected_directory if self.selected_directory else os.path.expanduser('~')
        files, _ = QFileDialog.getOpenFileNames(
            self,
            '选择图片文件',
            base_dir,
            'Image Files (*.jpg *.jpeg *.png *.bmp *.webp)',
        )
        if files:
            self.selected_files = files
            self.selected_directory = None
            self.dir_label.setText('未选择目录')
            self.file_label.setText(os.path.basename(files[0]) if len(files) == 1 else f'已选择 {len(files)} 个文件')
            self.compress_button.setEnabled(True)
            self.save_settings()

    def toggle_quality_settings(self, _index):
        enabled = self.format_combo.currentData() == 'jpeg'
        self.quality_slider.setEnabled(enabled)
        self.quality_value.setEnabled(enabled)

    def toggle_rename_settings(self, enabled):
        self.prefix_input.setEnabled(enabled)
        self.separator_combo.setEnabled(enabled)
        self.number_spinbox.setEnabled(enabled)

    def start_compression(self):
        if not self.selected_directory and not self.selected_files:
            QMessageBox.warning(self, '警告', '请先选择目录或文件。')
            return

        self.results_table.setRowCount(0)
        self.progress_bar.setValue(0)
        self.progress_bar.setVisible(True)

        self.compress_button.setEnabled(False)
        self.dir_button.setEnabled(False)
        self.file_button.setEnabled(False)
        self.format_combo.setEnabled(False)
        self.quality_slider.setEnabled(False)
        self.quality_value.setEnabled(False)
        self.rename_checkbox.setEnabled(False)
        self.prefix_input.setEnabled(False)
        self.separator_combo.setEnabled(False)
        self.number_spinbox.setEnabled(False)

        quality = self.quality_value.value()
        output_format = self.format_combo.currentData()

        rename_settings = None
        if self.rename_checkbox.isChecked():
            rename_settings = {
                'enabled': True,
                'prefix': self.prefix_input.text(),
                'separator': self.separator_combo.currentData(),
                'start_number': self.number_spinbox.value(),
            }

        if self.selected_directory:
            self.compression_thread = CompressionThread(
                directory=self.selected_directory,
                output_format=output_format,
                quality=quality,
                rename_settings=rename_settings,
            )
        else:
            self.compression_thread = CompressionThread(
                files=self.selected_files,
                output_format=output_format,
                quality=quality,
                rename_settings=rename_settings,
            )

        self.compression_thread.progress_signal.connect(self.update_progress)
        self.compression_thread.finished_signal.connect(self.compression_finished)
        self.compression_thread.start()
        self.save_settings()

    def update_progress(self, processed, total, result):
        progress = int((processed / total) * 100) if total > 0 else 0
        self.progress_bar.setValue(progress)

        if result and result.get('success', False):
            row = self.results_table.rowCount()
            self.results_table.insertRow(row)
            self.results_table.setItem(row, 0, QTableWidgetItem(os.path.basename(result['original_path'])))
            self.results_table.setItem(row, 1, QTableWidgetItem(result['original_size_formatted']))
            self.results_table.setItem(row, 2, QTableWidgetItem(result['compressed_size_formatted']))
            self.results_table.setItem(row, 3, QTableWidgetItem(f"{result['compression_ratio']:.2f}%"))

    def compression_finished(self, _results, summary):
        self.total_files_label.setText(f"总文件数: {summary['total_files']}")
        self.total_original_size_label.setText(f"压缩前总大小: {summary['total_original_size_formatted']}")
        self.total_compressed_size_label.setText(f"压缩后总大小: {summary['total_compressed_size_formatted']}")
        self.overall_ratio_label.setText(f"总体压缩比例: {summary['overall_compression_ratio']:.2f}%")

        self.compress_button.setEnabled(True)
        self.dir_button.setEnabled(True)
        self.file_button.setEnabled(True)
        self.format_combo.setEnabled(True)
        self.toggle_quality_settings(self.format_combo.currentIndex())
        self.rename_checkbox.setEnabled(True)
        self.toggle_rename_settings(self.rename_checkbox.isChecked())
        self.progress_bar.setVisible(False)

        QMessageBox.information(
            self,
            '压缩完成',
            f"已压缩 {summary['processed_files']} 个图片文件\n"
            f"总大小从 {summary['total_original_size_formatted']} "
            f"减小到 {summary['total_compressed_size_formatted']} "
            f"(减小了 {summary['overall_compression_ratio']:.2f}%)",
        )
        self.save_settings()

    def closeEvent(self, event: QCloseEvent):
        self.save_settings()
        super().closeEvent(event)

