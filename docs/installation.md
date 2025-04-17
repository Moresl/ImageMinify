# 安装指南

本文档提供了图片压缩工具的详细安装说明。

## 系统要求

- **操作系统**：Windows 7/8/10/11
- **Python版本**（仅从源码安装时需要）：Python 3.8或更高版本
- **磁盘空间**：约100MB

## 方法1：使用预编译的可执行文件（推荐）

这是最简单的安装方法，不需要安装Python或任何依赖项。

1. 访问[GitHub Releases页面](https://github.com/yourusername/img-yasuo/releases)
2. 下载最新版本的`图片压缩工具_vX.X.X.zip`文件
3. 解压缩下载的文件到任意位置
4. 双击`图片压缩工具.exe`运行程序

## 方法2：从源代码安装

如果您想从源代码安装或进行开发，请按照以下步骤操作：

### 先决条件

- Python 3.8或更高版本
- pip（Python包管理器）
- Git（可选，用于克隆仓库）

### 步骤

1. **克隆或下载仓库**

   ```bash
   # 使用Git克隆
   git clone https://github.com/yourusername/img-yasuo.git
   cd img-yasuo
   
   # 或者下载ZIP文件并解压
   # https://github.com/yourusername/img-yasuo/archive/refs/heads/main.zip
   ```

2. **创建虚拟环境（可选但推荐）**

   ```bash
   # 创建虚拟环境
   python -m venv venv
   
   # 在Windows上激活虚拟环境
   venv\Scripts\activate
   
   # 在Linux/Mac上激活虚拟环境
   # source venv/bin/activate
   ```

3. **安装依赖项**

   ```bash
   pip install -r requirements.txt
   ```

4. **运行程序**

   ```bash
   python main.py
   ```

## 方法3：构建自己的可执行文件

如果您想自己构建可执行文件，请按照以下步骤操作：

1. 按照"从源代码安装"的步骤1-3进行操作
2. 安装PyInstaller

   ```bash
   pip install pyinstaller
   ```

3. 运行构建脚本

   ```bash
   python build.py
   ```

4. 构建完成后，可执行文件将位于`dist`目录中

## 故障排除

### 常见问题

1. **程序无法启动**
   - 确保您的Windows版本是受支持的版本
   - 尝试以管理员身份运行程序
   - 检查是否安装了所有必要的Visual C++ Redistributable包

2. **缺少DLL错误**
   - 下载并安装最新的[Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)

3. **从源代码安装时的依赖项错误**
   - 确保您使用的是Python 3.8或更高版本
   - 尝试更新pip：`python -m pip install --upgrade pip`
   - 逐个安装依赖项，查看具体错误信息

如果您遇到其他问题，请在[GitHub Issues](https://github.com/yourusername/img-yasuo/issues)页面报告。
