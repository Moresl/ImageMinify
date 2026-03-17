# 更新日志

所有项目的显著变更都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
并且本项目遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## [2.1.0] - 2026-03-17

### 优化

- 更换全新应用图标，多尺寸 ICO（16/32/48/256）确保各处正确显示
- 按钮去除 WPF-UI hover 变白效果，使用自定义 ControlTemplate
- 「开始压缩」按钮移入独立的「执行操作」GroupBox，布局更清晰
- 输入控件尺寸统一优化（前缀输入框、ComboBox 等）
- 窗口基类从 FluentWindow 改为 Window，提升兼容性

## [2.0.0] - 2026-03-17

### 重大变更

- **技术栈重构**：从 Python + PyQt5 迁移至 C# + WPF (.NET 10)
- 采用 MVVM 架构（CommunityToolkit.Mvvm）
- 使用 WPF-UI 实现 Fluent Design 风格
- 图像处理改用 SixLabors.ImageSharp

### 添加

- EXIF 元数据读取和保留（MetadataExtractor）
- 设置持久化（Windows Registry）
- 完整的单元测试覆盖
- 原生 imagequant 量化支持（NativeLibraryLoader）
- 独立的 PNG/JPEG/WebP 压缩服务

### 变更

- 项目结构重组为 Models/Services/ViewModels/Views
- 界面升级为 Fluent Design 风格
- 构建系统改为 .NET SDK（dotnet build/publish）

## [1.1.0] - 2026-02-06

### 添加

- PNG 压缩链路升级：优先 imagequant 量化 + Floyd-Steinberg 抖动
- PNG 无损二次优化：支持 oxipng（可用时自动启用）
- JPEG 压缩链路升级：支持 MozJPEG 无损优化（可用时自动启用）
- UI 新增压缩引擎能力展示标签
- UI 支持接入 PyQt-Fluent-Widgets 组件风格（可选）

### 变更

- `requirements.txt` 增加 `imagequant`、`mozjpeg-lossless-optimization`、`PyQt-Fluent-Widgets`
- `setup.py` 同步新增上述依赖

## [1.0.0] - 2025-04-17

### 添加

- 初始版本发布
- 支持 JPEG、PNG、WebP 等格式的图像压缩
- 支持选择目录或特定文件进行压缩
- 支持文件重命名功能
- 支持格式转换
- 详细的压缩统计信息
- 中文用户界面
- Windows 可执行文件

## [0.9.0] - 2025-04-15

### 添加

- 测试版本
- 基本压缩功能
- 简单的用户界面
