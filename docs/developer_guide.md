# 开发者文档

本文档提供了图片压缩工具的技术细节和开发指南，适用于想要理解、修改或扩展项目的开发者。

## 项目结构

```
img-yasuo/
├── main.py              # 主入口点
├── compressor.py        # 图像压缩逻辑
├── ui.py                # 用户界面
├── build.py             # 构建脚本
├── requirements.txt     # 依赖项
├── icon.ico             # 应用图标
├── tests/               # 测试文件
│   ├── __init__.py
│   ├── test_compressor.py
│   └── test_ui.py
├── docs/                # 文档
└── README.md            # 项目说明
```

## 核心模块

### main.py

主程序入口点，初始化应用程序和UI。

### compressor.py

包含所有图像处理和压缩逻辑：

- `ImageCompressor` 类：主要的压缩引擎
  - `compress_image()`: 压缩单个图像
  - `compress_directory()`: 批量压缩目录中的图像
  - `compress_png_advanced()`: 高级PNG压缩

### ui.py

包含用户界面相关代码：

- `ImageCompressorUI` 类：主窗口和UI逻辑
- `CompressionThread` 类：后台处理线程

## 技术实现

### 图像压缩

图像压缩使用以下技术：

1. **JPEG压缩**：
   - 使用Pillow的`save()`方法，调整质量参数
   - 移除EXIF元数据减小文件大小
   - 使用`optimize=True`参数进一步优化

2. **PNG压缩**：
   - 使用最高级别的压缩（`compress_level=9`）
   - 对非透明图像进行颜色量化（减少到256色）
   - 保留透明通道（如果有）

3. **WebP压缩**：
   - 使用无损模式（`lossless=True`）
   - 设置最高质量（`quality=100`）

### 多线程处理

使用PyQt5的`QThread`实现多线程处理：

- 主线程处理UI交互
- 后台线程执行图像压缩
- 使用信号/槽机制更新UI和传递结果

### 文件重命名

文件重命名功能实现：

- 使用用户提供的前缀和分隔符
- 根据文件总数自动计算数字位数
- 使用`zfill()`添加前导零
- 使用`os.rename()`重命名文件

## 扩展指南

### 添加新的压缩算法

要添加新的压缩算法，请按照以下步骤操作：

1. 在`compressor.py`中添加新的压缩方法
2. 在`compress_image()`方法中添加对新算法的调用
3. 更新UI以提供新算法的选项

示例：

```python
def compress_image_new_algorithm(self, input_path, output_path):
    # 实现新的压缩算法
    pass
```

### 添加新的输出格式

要添加新的输出格式，请按照以下步骤操作：

1. 在`ui.py`的`format_combo`中添加新格式
2. 在`compressor.py`的`compress_image()`方法中添加对新格式的处理

### 添加新的UI功能

要添加新的UI功能，请按照以下步骤操作：

1. 在`ui.py`的`init_ui()`方法中添加新的UI元素
2. 添加相应的事件处理方法
3. 更新`compressor.py`以支持新功能

## 测试

项目使用Python的`unittest`框架进行测试。测试文件位于`tests/`目录中。

运行测试：

```bash
python -m unittest discover tests
```

### 添加新测试

添加新测试时，请遵循以下命名约定：

- 测试文件名：`test_<module>.py`
- 测试类名：`Test<Class>`
- 测试方法名：`test_<method>`

示例：

```python
import unittest
from compressor import ImageCompressor

class TestImageCompressor(unittest.TestCase):
    def test_compress_image(self):
        # 测试代码
        pass
```

## 构建

项目使用PyInstaller构建可执行文件。构建脚本位于`build.py`中。

自定义构建过程：

1. 修改`build.py`中的PyInstaller参数
2. 运行构建脚本：`python build.py`

## 调试技巧

1. 启用详细日志：

```python
import logging
logging.basicConfig(level=logging.DEBUG)
```

2. 使用PyQt5的调试工具：

```python
from PyQt5.QtCore import pyqtRemoveInputHook
import pdb

pyqtRemoveInputHook()
pdb.set_trace()  # 设置断点
```

3. 检查图像处理过程：

```python
# 在compress_image方法中添加
img.save("debug_output.png")  # 保存中间结果
```

## 性能优化

- 使用`cProfile`分析性能瓶颈
- 考虑使用`numpy`加速图像处理
- 对大图像使用分块处理减少内存使用
