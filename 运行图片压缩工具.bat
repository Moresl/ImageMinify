@echo off
echo 正在启动图片压缩工具...

REM 检查可执行文件是否存在
if exist "dist\图片压缩工具.exe" (
    start "" "dist\图片压缩工具.exe"
) else if exist "图片压缩工具.exe" (
    start "" "图片压缩工具.exe"
) else (
    echo 错误：找不到可执行文件！
    echo 请确保"图片压缩工具.exe"文件在当前目录或dist目录中。
    pause
)
