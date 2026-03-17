# 贡献指南

感谢您考虑为 ImageMinify 做出贡献！这个文档提供了贡献代码的指南和流程。

## 行为准则

请阅读并遵守我们的[行为准则](CODE_OF_CONDUCT.md)。

## 如何贡献

### 报告 Bug

如果您发现了 Bug，请通过 GitHub Issues 报告，并包含以下信息：

1. 问题的简要描述
2. 重现步骤
3. 预期行为
4. 实际行为
5. 截图（如果适用）
6. 您的操作系统版本和 .NET 版本
7. 其他可能有帮助的信息

### 提出新功能

如果您有新功能的想法，请先通过 GitHub Issues 讨论。这样可以确保您的工作不会与其他人的工作重复，并且符合项目的目标和方向。

### 提交 Pull Request

1. Fork 仓库
2. 创建您的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交您的更改 (`git commit -m 'feat: add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开 Pull Request

### 开发环境设置

**前置要求**：

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 17.x+ 或 VS Code + C# Dev Kit
- Windows 10/11（WPF 需要 Windows 平台）

**开始开发**：

```bash
# 克隆仓库
git clone https://github.com/Moresl/ImageMinify
cd ImageMinify

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行程序
dotnet run
```

### 开发流程

1. 确保您的代码遵循项目的代码风格
2. 更新文档以反映任何更改
3. 确保您的代码通过所有测试
4. 如果添加新功能，请添加相应的测试
5. 确保代码不会引入新的警告或错误

## 代码风格

- 遵循 [C# 编码约定](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用有意义的变量名和方法名
- MVVM 模式：视图逻辑放在 ViewModel，业务逻辑放在 Services
- 使用 `CommunityToolkit.Mvvm` 的特性（`[ObservableProperty]`、`[RelayCommand]` 等）
- 保持代码简洁明了

## 项目架构

```
Models/       → 数据模型（纯 POCO）
Services/     → 业务逻辑（图像压缩、文件操作、设置管理）
ViewModels/   → 视图模型（MVVM，连接 View 和 Service）
Views/        → XAML 视图
Helpers/      → 工具类（转换器等）
```

## 测试

在提交代码之前，请确保运行测试并确保它们通过：

```bash
dotnet test
```

测试项目位于 `tests/ImageMinify.Tests/`。

## 文档

如果您的更改影响了用户体验或添加了新功能，请更新相应的文档。

## 许可证

通过贡献您的代码，您同意您的贡献将根据项目的 [MIT 许可证](LICENSE) 进行许可。
