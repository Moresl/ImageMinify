# 常见问题解答 (FAQ)

## 一般问题

### 这个工具是免费的吗？

是的，图片压缩工具是完全免费和开源的，根据MIT许可证发布。

### 支持哪些操作系统？

目前，图片压缩工具主要支持Windows系统（Windows 7/8/10/11）。未来可能会添加对macOS和Linux的支持。

### 是否需要安装Python？

如果您使用预编译的可执行文件（.exe），则不需要安装Python。如果您想从源代码运行，则需要Python 3.8或更高版本。

## 功能问题

### 支持哪些图片格式？

工具支持以下格式：
- 输入格式：JPEG (.jpg, .jpeg), PNG (.png), BMP (.bmp), WebP (.webp)
- 输出格式：JPEG, PNG, WebP

### 压缩是否会降低图片质量？

这取决于您选择的输出格式和设置：
- JPEG：是有损压缩，可能会降低质量，但您可以通过质量滑块控制
- PNG：使用无损压缩，不会降低质量
- WebP：使用无损模式，不会降低质量

### 压缩后的文件保存在哪里？

压缩后的文件保存在原文件所在的目录中：
- 如果未启用重命名，文件名会添加"_compressed"后缀
- 如果启用了重命名，文件将按照指定的格式重命名

### 有文件大小限制吗？

理论上没有严格的文件大小限制，但处理非常大的图片（如超过50MB的图片）可能会导致内存使用增加和处理时间延长。

### 可以一次处理多少张图片？

没有固定限制，但建议一次处理不超过1000张图片，以避免可能的内存问题。

## 技术问题

### 为什么某些PNG文件压缩效果不明显？

PNG是一种已经压缩的格式，对于某些PNG文件（特别是已经优化过的文件或包含大量不同颜色的图片），进一步压缩的空间可能有限。

### 为什么我的杀毒软件报警？

某些杀毒软件可能会对PyInstaller打包的程序产生误报。这是因为PyInstaller的打包方式与某些恶意软件使用的技术类似。我们的软件是完全安全的，您可以查看源代码进行验证。

### 应用程序崩溃了，我该怎么办？

如果应用程序崩溃，请尝试以下步骤：
1. 重新启动应用程序
2. 尝试处理较少的文件
3. 检查是否有足够的磁盘空间
4. 如果问题持续存在，请在GitHub Issues页面报告问题，并提供详细的错误信息和重现步骤

### 如何报告bug或请求新功能？

请访问我们的[GitHub Issues页面](https://github.com/yourusername/img-yasuo/issues)报告bug或请求新功能。请提供尽可能详细的信息，以帮助我们理解和解决问题。

## 开发问题

### 如何贡献代码？

请查看我们的[贡献指南](../CONTRIBUTING.md)了解如何贡献代码。

### 我可以在自己的项目中使用这个工具的代码吗？

是的，根据MIT许可证，您可以在自己的项目中使用、修改和分发这个工具的代码，只需保留原始版权声明和许可证文本。

### 如何构建自己的版本？

请查看[开发者文档](developer_guide.md)了解如何构建自己的版本。
