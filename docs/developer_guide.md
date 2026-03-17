# 开发者文档

本文档提供了 ImageMinify 的技术细节和开发指南。

## 技术栈

| 组件 | 技术 |
|------|------|
| 运行时 | .NET 10 |
| UI 框架 | WPF + [WPF-UI](https://github.com/lepoco/wpfui) |
| MVVM | [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) |
| 图像处理 | [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) |
| EXIF 读取 | [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) |
| 测试 | xUnit + Moq |

## 项目结构

```
ImageMinify/
├── App.xaml                        # WPF 应用定义
├── App.xaml.cs                     # 应用启动逻辑
├── GlobalUsings.cs                 # 全局 using 声明
├── ImageMinify.csproj              # 项目文件
├── ImageMinify.sln                 # 解决方案文件
│
├── Models/                         # 数据模型层
│   ├── AppSettingsSnapshot.cs      # 应用设置快照
│   ├── CompressionResult.cs        # 单张图片压缩结果
│   ├── CompressionSummary.cs       # 批量压缩统计
│   ├── EngineCapabilities.cs       # 压缩引擎能力
│   ├── RenameSettings.cs           # 重命名配置
│   └── StatusSeverity.cs           # 状态级别枚举
│
├── Services/                       # 业务逻辑层
│   ├── IImageCompressor.cs         # 压缩器接口
│   ├── ImageCompressor.cs          # 图像压缩主服务（门面）
│   ├── PngCompressor.cs            # PNG 压缩策略
│   ├── JpegCompressor.cs           # JPEG 压缩策略
│   ├── WebpCompressor.cs           # WebP 压缩策略
│   ├── ImagequantNativeQuantizer.cs # imagequant 原生调用
│   ├── ExifService.cs              # EXIF 元数据服务
│   ├── FileRenameService.cs        # 文件重命名服务
│   ├── ISettingsStore.cs           # 设置存储接口
│   ├── WindowsSettingsStore.cs     # Windows 注册表存储实现
│   └── SettingsService.cs          # 设置管理服务
│
├── ViewModels/                     # 视图模型层
│   └── MainViewModel.cs            # 主视图模型（UI 逻辑）
│
├── Views/                          # 视图层
│   └── MainWindow.xaml(.cs)        # 主窗口
│
├── Helpers/                        # 工具类
│   ├── BoolToVisibilityConverter.cs # 布尔转可见性转换器
│   ├── FileSizeFormatter.cs        # 文件大小格式化
│   └── NativeLibraryLoader.cs      # 原生库加载器
│
├── Assets/                         # 资源文件
│   └── icon.ico                    # 应用图标
│
└── tests/ImageMinify.Tests/        # 单元测试
    ├── ImageCompressorTests.cs
    ├── PngCompressorTests.cs
    ├── JpegCompressorTests.cs
    ├── FileRenameServiceTests.cs
    ├── FileSizeFormatterTests.cs
    ├── MainViewModelTests.cs
    ├── SettingsServiceTests.cs
    └── InMemorySettingsStore.cs    # 测试用内存存储
```

## 架构设计

### MVVM 模式

```
View (XAML) ←→ ViewModel (CommunityToolkit.Mvvm) → Services → Models
```

- **View**：纯 XAML 视图，通过数据绑定与 ViewModel 交互
- **ViewModel**：使用 `[ObservableProperty]` 和 `[RelayCommand]` 管理 UI 状态和命令
- **Services**：业务逻辑，与 UI 无关
- **Models**：纯数据类，无逻辑

### 压缩服务架构

`ImageCompressor` 作为门面（Facade），内部委托给具体的格式压缩器：

```
ImageCompressor (Facade)
├── PngCompressor       → ImageSharp + imagequant（可选）
├── JpegCompressor      → ImageSharp + MozJPEG（可选）
└── WebpCompressor      → ImageSharp
```

### 设置持久化

通过接口抽象实现可测试性：

```
ISettingsStore (接口)
├── WindowsSettingsStore  → Windows 注册表（生产环境）
└── InMemorySettingsStore → 内存存储（测试环境）
```

## 开发指南

### 环境配置

```bash
# 还原依赖
dotnet restore

# 构建
dotnet build

# 运行
dotnet run

# 运行测试
dotnet test
```

### 添加新的图片格式支持

1. 在 `Services/` 下创建新的压缩器类（如 `AvifCompressor.cs`）
2. 在 `ImageCompressor` 中注册新格式
3. 在 `MainViewModel` 中添加 UI 选项
4. 添加对应的单元测试

### 添加新的设置项

1. 在 `Models/AppSettingsSnapshot.cs` 中添加属性
2. 在 `SettingsService` 中添加加载/保存逻辑
3. 在 `MainViewModel` 中绑定到 UI

### 测试

项目使用 xUnit 测试框架：

```bash
# 运行所有测试
dotnet test

# 运行特定测试
dotnet test --filter "FullyQualifiedName~PngCompressorTests"
```

## NuGet 依赖

| 包 | 用途 |
|----|------|
| CommunityToolkit.Mvvm 8.2.2 | MVVM 框架 |
| MetadataExtractor 2.8.1 | EXIF 元数据读取 |
| SixLabors.ImageSharp 3.1.5 | 图像处理 |
| WPF-UI 4.2.0 | Fluent Design 控件库 |

## 版本历史

项目经历了以下技术演变：

| 版本 | 技术栈 | 说明 |
|------|--------|------|
| v0.9 - v1.1 | Python + PyQt5 | 初始版本 |
| v2.0 | C# + WPF + .NET 10 | 完全重构 |
