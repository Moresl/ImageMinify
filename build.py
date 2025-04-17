import os
import sys
import shutil
import PyInstaller.__main__

# 清理之前的构建文件
if os.path.exists('dist'):
    shutil.rmtree('dist')
if os.path.exists('build'):
    shutil.rmtree('build')

# 确保图标文件存在
icon_path = 'icon.ico'
if not os.path.exists(icon_path):
    print(f"警告: 图标文件 {icon_path} 不存在，将使用默认图标")
    icon_path = None

# 设置PyInstaller参数
args = [
    'main.py',  # 主脚本
    '--name=图片压缩工具',  # 应用程序名称
    '--onefile',  # 打包为单个可执行文件
    '--windowed',  # 使用窗口模式（不显示控制台）
    '--clean',  # 清理临时文件
    '--add-data=icon.ico;.',  # 添加图标文件
]

# 如果有图标，添加图标参数
if icon_path:
    args.append(f'--icon={icon_path}')

# 运行PyInstaller
PyInstaller.__main__.run(args)

print("打包完成！可执行文件位于 dist 目录中。")
