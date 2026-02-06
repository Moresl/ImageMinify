# 更新日志

所有项目的显著变更都将记录在此文件中。

格式基于[Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
并且本项目遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## [1.0.0] - 2025-04-17

### 添加

- 初始版本发布
- 支持JPEG、PNG、WebP等格式的图像压缩
- 支持选择目录或特定文件进行压缩
- 支持文件重命名功能
- 支持格式转换
- 详细的压缩统计信息
- 中文用户界面
- Windows可执行文件

### 修复

- 无（首次发布）

### 变更

- 无（首次发布）

### 移除

- 无（首次发布）

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

## [0.9.0] - 2025-04-15

### 添加

- 测试版本
- 基本压缩功能
- 简单的用户界面

### 已知问题

- 某些PNG文件可能无法正确压缩
- 应用图标在某些Windows版本上可能不显示
- 处理大量文件时可能出现内存问题
