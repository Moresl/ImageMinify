# 贡献指南

感谢您考虑为图片压缩工具做出贡献！这个文档提供了贡献代码的指南和流程。

## 行为准则

请阅读并遵守我们的[行为准则](CODE_OF_CONDUCT.md)。

## 如何贡献

### 报告Bug

如果您发现了bug，请通过GitHub Issues报告，并包含以下信息：

1. 问题的简要描述
2. 重现步骤
3. 预期行为
4. 实际行为
5. 截图（如果适用）
6. 您的操作系统和Python版本
7. 其他可能有帮助的信息

### 提出新功能

如果您有新功能的想法，请先通过GitHub Issues讨论。这样可以确保您的工作不会与其他人的工作重复，并且符合项目的目标和方向。

### 提交Pull Request

1. Fork仓库
2. 创建您的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交您的更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开Pull Request

### 开发流程

1. 确保您的代码遵循项目的代码风格
2. 更新文档以反映任何更改
3. 确保您的代码通过所有测试
4. 如果添加新功能，请添加相应的测试
5. 确保您的代码不会引入新的警告或错误

## 代码风格

- 遵循[PEP 8](https://www.python.org/dev/peps/pep-0008/)风格指南
- 使用有意义的变量名和函数名
- 添加适当的注释和文档字符串
- 保持代码简洁明了

## 测试

在提交代码之前，请确保运行测试并确保它们通过：

```bash
python -m unittest discover tests
```

## 文档

如果您的更改影响了用户体验或添加了新功能，请更新相应的文档。

## 许可证

通过贡献您的代码，您同意您的贡献将根据项目的[MIT许可证](LICENSE)进行许可。
