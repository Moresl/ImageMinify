import os
import sys
from PyQt5.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout,
                            QHBoxLayout, QPushButton, QLabel, QFileDialog,
                            QProgressBar, QSlider, QTableWidget, QTableWidgetItem,
                            QHeaderView, QMessageBox, QGroupBox, QSpinBox,
                            QRadioButton, QButtonGroup, QComboBox, QLineEdit,
                            QCheckBox)
from PyQt5.QtCore import Qt, QThread, pyqtSignal
from PyQt5.QtGui import QPixmap, QIcon, QFont, QImage

from compressor import ImageCompressor

class CompressionThread(QThread):
    """Thread for running image compression in the background."""
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
                except Exception as e:
                    print(f"Error renaming file: {e}")

            self.progress_signal.emit(processed, total, result)

        if self.directory:
         
            results, summary = self.compressor.compress_directory(
                self.directory,
                output_format=self.output_format,
                quality=self.quality,
                callback=progress_callback
            )
        else:
            results = []
            total_original_size = 0
            total_compressed_size = 0
            total_files = len(self.files)

            for i, file_path in enumerate(self.files):
                result = self.compressor.compress_image(
                    file_path,
                    output_format=self.output_format,
                    quality=self.quality
                )

                if result and result['success']:
                    results.append(result)
                    total_original_size += result['original_size']
                    total_compressed_size += result['compressed_size']

                # 调用回调函数
                progress_callback(i + 1, total_files, result)

            # 计算总体统计信息
            overall_ratio = (1 - total_compressed_size / total_original_size) * 100 if total_original_size > 0 else 0

            summary = {
                'total_files': total_files,
                'processed_files': len(results),
                'total_original_size': total_original_size,
                'total_compressed_size': total_compressed_size,
                'total_original_size_formatted': self.compressor.get_formatted_size(total_original_size),
                'total_compressed_size_formatted': self.compressor.get_formatted_size(total_compressed_size),
                'overall_compression_ratio': overall_ratio
            }

        self.finished_signal.emit(results, summary)

class ImageCompressorUI(QMainWindow):
    def __init__(self):
        super().__init__()
        self.compressor = ImageCompressor()
        self.init_ui()

    def init_ui(self):
        # 设置中文字体
        font = QFont("Microsoft YaHei", 9)  # 使用微软雅黑字体
        self.setFont(font)

        # 设置应用图标
        app_icon = QIcon("icon.ico")
        self.setWindowIcon(app_icon)

        self.setWindowTitle('图片压缩工具')
        self.setGeometry(100, 100, 800, 600)

        # Main widget and layout
        main_widget = QWidget()
        main_layout = QVBoxLayout()

        # Directory and file selection
        dir_group = QGroupBox("选择文件")
        dir_layout = QVBoxLayout()

        # Directory selection
        dir_select_layout = QHBoxLayout()
        self.dir_label = QLabel("未选择目录")
        self.dir_button = QPushButton("选择目录")
        self.dir_button.clicked.connect(self.select_directory)

        dir_select_layout.addWidget(self.dir_label)
        dir_select_layout.addWidget(self.dir_button)

        # File selection
        file_select_layout = QHBoxLayout()
        self.file_label = QLabel("未选择文件")
        self.file_button = QPushButton("选择文件")
        self.file_button.clicked.connect(self.select_files)

        file_select_layout.addWidget(self.file_label)
        file_select_layout.addWidget(self.file_button)

        # Add both layouts to the group
        dir_layout.addLayout(dir_select_layout)
        dir_layout.addLayout(file_select_layout)
        dir_group.setLayout(dir_layout)

        # Compression settings
        settings_group = QGroupBox("压缩设置")
        settings_layout = QVBoxLayout()

        # Output format selection
        format_layout = QHBoxLayout()
        format_label = QLabel("输出格式:")

        self.format_combo = QComboBox()
        self.format_combo.addItem("保持原格式", "original")
        self.format_combo.addItem("JPEG", "jpeg")
        self.format_combo.addItem("PNG", "png")
        self.format_combo.addItem("WebP", "webp")

        format_layout.addWidget(format_label)
        format_layout.addWidget(self.format_combo)
        format_layout.addStretch(1)

        # Quality settings (for JPEG)
        quality_layout = QHBoxLayout()
        quality_label = QLabel("JPEG质量:")
        self.quality_slider = QSlider(Qt.Horizontal)
        self.quality_slider.setMinimum(1)
        self.quality_slider.setMaximum(100)
        self.quality_slider.setValue(85)
        self.quality_slider.setTickPosition(QSlider.TicksBelow)
        self.quality_slider.setTickInterval(10)

        self.quality_value = QSpinBox()
        self.quality_value.setMinimum(1)
        self.quality_value.setMaximum(100)
        self.quality_value.setValue(85)

        # Connect slider and spinbox
        self.quality_slider.valueChanged.connect(self.quality_value.setValue)
        self.quality_value.valueChanged.connect(self.quality_slider.setValue)

        # Connect format combo to enable/disable quality settings
        self.format_combo.currentIndexChanged.connect(self.toggle_quality_settings)

        quality_layout.addWidget(quality_label)
        quality_layout.addWidget(self.quality_slider)
        quality_layout.addWidget(self.quality_value)

        # Rename settings
        rename_group = QGroupBox("重命名设置")
        rename_layout = QVBoxLayout()

        # Enable rename checkbox
        self.rename_checkbox = QCheckBox("启用重命名")
        self.rename_checkbox.toggled.connect(self.toggle_rename_settings)

        # Prefix input
        prefix_layout = QHBoxLayout()
        prefix_label = QLabel("前缀:")
        self.prefix_input = QLineEdit()
        self.prefix_input.setPlaceholderText("输入文件名前缀")
        self.prefix_input.setEnabled(False)
        prefix_layout.addWidget(prefix_label)
        prefix_layout.addWidget(self.prefix_input)

        # Separator selection
        separator_layout = QHBoxLayout()
        separator_label = QLabel("分隔符:")
        self.separator_combo = QComboBox()
        self.separator_combo.addItem("-", "-")
        self.separator_combo.addItem("_", "_")
        self.separator_combo.setEnabled(False)
        separator_layout.addWidget(separator_label)
        separator_layout.addWidget(self.separator_combo)
        separator_layout.addStretch(1)

        # Start number
        number_layout = QHBoxLayout()
        number_label = QLabel("起始编号:")
        self.number_spinbox = QSpinBox()
        self.number_spinbox.setMinimum(1)
        self.number_spinbox.setMaximum(9999)
        self.number_spinbox.setValue(1)
        self.number_spinbox.setEnabled(False)
        number_layout.addWidget(number_label)
        number_layout.addWidget(self.number_spinbox)
        number_layout.addStretch(1)

        # Add all rename layouts
        rename_layout.addWidget(self.rename_checkbox)
        rename_layout.addLayout(prefix_layout)
        rename_layout.addLayout(separator_layout)
        rename_layout.addLayout(number_layout)
        rename_group.setLayout(rename_layout)

        # Add all layouts to settings group
        settings_layout.addLayout(format_layout)
        settings_layout.addLayout(quality_layout)
        settings_group.setLayout(settings_layout)

        # Compression button
        self.compress_button = QPushButton("开始压缩")
        self.compress_button.setMinimumHeight(50)  # 增加按钮高度
        self.compress_button.setStyleSheet("font-size: 14px; font-weight: bold;")  # 增加字体大小和粗细
        self.compress_button.clicked.connect(self.start_compression)
        self.compress_button.setEnabled(False)

        # Progress bar
        self.progress_bar = QProgressBar()
        self.progress_bar.setVisible(False)

        # Results table
        self.results_table = QTableWidget(0, 4)
        self.results_table.setHorizontalHeaderLabels([
            "文件名", "原始大小", "压缩后大小", "压缩比例"
        ])
        self.results_table.horizontalHeader().setSectionResizeMode(0, QHeaderView.Stretch)

        # Summary section
        summary_group = QGroupBox("压缩结果汇总")
        self.summary_layout = QVBoxLayout()

        self.total_files_label = QLabel("总文件数: 0")
        self.total_original_size_label = QLabel("压缩前总大小: 0 B")
        self.total_compressed_size_label = QLabel("压缩后总大小: 0 B")
        self.overall_ratio_label = QLabel("总体压缩比例: 0%")

        self.summary_layout.addWidget(self.total_files_label)
        self.summary_layout.addWidget(self.total_original_size_label)
        self.summary_layout.addWidget(self.total_compressed_size_label)
        self.summary_layout.addWidget(self.overall_ratio_label)

        summary_group.setLayout(self.summary_layout)

        # Add all widgets to main layout
        main_layout.addWidget(dir_group)
        main_layout.addWidget(settings_group)
        main_layout.addWidget(rename_group)
        main_layout.addWidget(self.compress_button)
        main_layout.addWidget(self.progress_bar)
        main_layout.addWidget(self.results_table)
        main_layout.addWidget(summary_group)

        main_widget.setLayout(main_layout)
        self.setCentralWidget(main_widget)

    def select_directory(self):
        directory = QFileDialog.getExistingDirectory(
            self, "选择目录", os.path.expanduser("~")
        )

        if directory:
            self.dir_label.setText(directory)
            self.compress_button.setEnabled(True)
            self.selected_directory = directory
            # 清除已选择的文件
            if hasattr(self, 'selected_files'):
                delattr(self, 'selected_files')
            self.file_label.setText("未选择文件")

    def select_files(self):
        files, _ = QFileDialog.getOpenFileNames(
            self,
            "选择图片文件",
            os.path.expanduser("~"),
            "Image Files (*.jpg *.jpeg *.png *.bmp *.webp)"
        )

        if files:
            self.selected_files = files
            # 清除已选择的目录
            if hasattr(self, 'selected_directory'):
                delattr(self, 'selected_directory')
            self.dir_label.setText("未选择目录")

            # 显示选择的文件数量
            if len(files) == 1:
                self.file_label.setText(os.path.basename(files[0]))
            else:
                self.file_label.setText(f"已选择 {len(files)} 个文件")

            self.compress_button.setEnabled(True)

    def toggle_quality_settings(self, index):
        # Enable or disable quality settings based on output format
        # Only enable quality settings for JPEG format
        format_data = self.format_combo.currentData()
        self.quality_slider.setEnabled(format_data == "jpeg")
        self.quality_value.setEnabled(format_data == "jpeg")

    def toggle_rename_settings(self, enabled):
        # Enable or disable rename settings based on checkbox
        self.prefix_input.setEnabled(enabled)
        self.separator_combo.setEnabled(enabled)
        self.number_spinbox.setEnabled(enabled)

    def start_compression(self):
        if not hasattr(self, 'selected_directory') and not hasattr(self, 'selected_files'):
            QMessageBox.warning(self, "警告", "请先选择目录或文件。")
            return

        # Clear previous results
        self.results_table.setRowCount(0)
        self.progress_bar.setValue(0)
        self.progress_bar.setVisible(True)

        # Disable UI elements during compression
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

        # Get compression settings
        quality = self.quality_value.value()
        output_format = self.format_combo.currentData()

        # Get rename settings
        rename_settings = None
        if self.rename_checkbox.isChecked():
            rename_settings = {
                'enabled': True,
                'prefix': self.prefix_input.text(),
                'separator': self.separator_combo.currentData(),
                'start_number': self.number_spinbox.value()
            }

        # Start compression in a separate thread
        if hasattr(self, 'selected_directory'):
            # 如果选择了目录
            self.compression_thread = CompressionThread(
                directory=self.selected_directory,
                output_format=output_format,
                quality=quality,
                rename_settings=rename_settings
            )
        else:
            # 如果选择了文件
            self.compression_thread = CompressionThread(
                files=self.selected_files,
                output_format=output_format,
                quality=quality,
                rename_settings=rename_settings
            )

        self.compression_thread.progress_signal.connect(self.update_progress)
        self.compression_thread.finished_signal.connect(self.compression_finished)
        self.compression_thread.start()

    def update_progress(self, processed, total, result):
        # Update progress bar
        progress = int((processed / total) * 100) if total > 0 else 0
        self.progress_bar.setValue(progress)

        # Add result to table if successful
        if result and result.get('success', False):
            row = self.results_table.rowCount()
            self.results_table.insertRow(row)

            file_name = os.path.basename(result['original_path'])
            original_size = result['original_size_formatted']
            compressed_size = result['compressed_size_formatted']
            ratio = f"{result['compression_ratio']:.2f}%"

            self.results_table.setItem(row, 0, QTableWidgetItem(file_name))
            self.results_table.setItem(row, 1, QTableWidgetItem(original_size))
            self.results_table.setItem(row, 2, QTableWidgetItem(compressed_size))
            self.results_table.setItem(row, 3, QTableWidgetItem(ratio))

    def compression_finished(self, results, summary):
        # Update summary
        self.total_files_label.setText(f"总文件数: {summary['total_files']}")
        self.total_original_size_label.setText(f"压缩前总大小: {summary['total_original_size_formatted']}")
        self.total_compressed_size_label.setText(f"压缩后总大小: {summary['total_compressed_size_formatted']}")
        self.overall_ratio_label.setText(f"总体压缩比例: {summary['overall_compression_ratio']:.2f}%")

        # Re-enable UI elements
        self.compress_button.setEnabled(True)
        self.dir_button.setEnabled(True)
        self.file_button.setEnabled(True)
        self.format_combo.setEnabled(True)
        self.toggle_quality_settings(self.format_combo.currentIndex())
        self.rename_checkbox.setEnabled(True)
        self.toggle_rename_settings(self.rename_checkbox.isChecked())

        # Hide progress bar
        self.progress_bar.setVisible(False)

        # Show completion message
        QMessageBox.information(
            self,
            "压缩完成",
            f"已压缩 {summary['processed_files']} 个图片文件\n"
            f"总大小从 {summary['total_original_size_formatted']} "
            f"减小到 {summary['total_compressed_size_formatted']} "
            f"(减小了 {summary['overall_compression_ratio']:.2f}%)"
        )
