# 图片压缩工具 (Image Compressor)

![License](https://img.shields.io/github/license/yourusername/img-yasuo)
![Python Version](https://img.shields.io/badge/python-3.8%2B-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

一个简单高效的图片压缩工具，支持多种格式，提供无损压缩选项，并具有批量处理和文件重命名功能。

## 功能特点

- **多格式支持**：支持JPEG、PNG、WebP等常见图片格式
- **批量处理**：可选择整个目录或特定图片文件进行压缩
- **格式转换**：可将图片转换为JPEG、PNG或WebP格式
- **无损压缩**：对PNG图像进行高效无损压缩
- **文件重命名**：支持自定义前缀、分隔符和序号
- **详细统计**：显示压缩前后的文件大小和压缩比例
- **简洁界面**：简单易用的图形用户界面

## 截图
![image](https://github.com/user-attachments/assets/a11d90d1-fff5-461e-aad1-2ea72c08778d)

## 安装方法

### 方法1：下载可执行文件

1. 从[Releases](https://github.com/Moresl/ImageMinify/releases)页面下载最新版本的可执行文件
2. 解压缩下载的文件
3. 双击`图片压缩工具.exe`运行程序

### 方法2：从源代码安装

```bash
# 克隆仓库
git clone https://github.com/Moresl/ImageMinify
cd img-yasuo

# 安装依赖
pip install -r requirements.txt

# 运行程序
python main.py
```

## 使用方法

1. **选择文件或目录**：
   - 点击"选择目录"按钮选择包含图片的文件夹，或
   - 点击"选择文件"按钮选择一个或多个图片文件

2. **选择输出格式**：
   - 保持原格式：保持原始格式但进行优化
   - JPEG：转换为JPEG格式（可调整质量）
   - PNG：转换为优化的PNG格式
   - WebP：转换为WebP格式

3. **调整设置**：
   - 如果选择JPEG格式，可以调整质量滑块
   - 如果需要重命名文件，勾选"启用重命名"并设置相关选项

4. **开始压缩**：
   - 点击"开始压缩"按钮开始处理
   - 处理完成后，可以在表格中查看每个文件的压缩结果

## 构建可执行文件

```bash
# 安装PyInstaller
pip install pyinstaller

# 运行构建脚本
python build.py
```

生成的可执行文件将位于`dist`目录中。

## 技术细节

- 使用PyQt5构建图形界面
- 使用Pillow (PIL)进行图像处理
- 对PNG图像使用高级压缩算法
- 多线程处理，保持界面响应性

## 贡献指南

欢迎贡献代码、报告问题或提出改进建议！请查看[贡献指南](CONTRIBUTING.md)了解详情。

## 许可证

本项目采用MIT许可证 - 详见[LICENSE](LICENSE)文件。

## 致谢

- [PyQt5](https://www.riverbankcomputing.com/software/pyqt/)
- [Pillow](https://python-pillow.org/)
- [PyInstaller](https://www.pyinstaller.org/)
  
<a href="https://github.com/Moresl/ImageMinify/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Moresl/ImageMinify" />
</a>

Made with [contrib.rocks](https://contrib.rocks).

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=Moresl/ImageMinify&type=Date)](https://www.star-history.com/#Moresl/ImageMinify&Date)
