# ImageMinify

![License](https://img.shields.io/github/license/moresl/ImageMinify)
![Python Version](https://img.shields.io/badge/python-3.8%2B-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

[中文](README.md) | English

A simple and efficient image compression tool that supports multiple formats, provides high-quality PNG/JPEG compression, and features batch processing and file renaming capabilities.

## Features

- **Multiple Format Support**: Supports common image formats such as JPEG, PNG, WebP, etc.
- **Batch Processing**: Select an entire directory or specific image files for compression
- **Format Conversion**: Convert images to JPEG, PNG, or WebP format
- **PNG Quantization**: Prioritizes imagequant with Floyd-Steinberg dithering
- **Lossless Optimization**: Supports PNG post-optimization via oxipng (when available)
- **JPEG Optimization**: Supports MozJPEG lossless optimization (when available)
- **Fluent UI**: Supports Fluent Widgets style integration
- **File Renaming**: Support for custom prefixes, separators, and sequential numbering
- **Detailed Statistics**: Display file sizes before and after compression and compression ratios
- **Clean Interface**: Simple and user-friendly graphical interface

## Screenshot
![image](https://github.com/user-attachments/assets/a11d90d1-fff5-461e-aad1-2ea72c08778d)

## Installation

### Method 1: Download Executable

1. Download the latest version from the [Releases](https://github.com/Moresl/ImageMinify/releases) page
2. Extract the downloaded file
3. Double-click `图片压缩工具.exe` to run the program

### Method 2: Install from Source

```bash
# Clone the repository
git clone https://github.com/Moresl/ImageMinify
cd ImageMinify

# Install dependencies
pip install -r requirements.txt

# Run the program
python main.py
```

## Usage

1. **Select Files or Directory**:
   - Click the "Select Directory" button to choose a folder containing images, or
   - Click the "Select Files" button to choose one or more specific image files

2. **Choose Output Format**:
   - Keep Original Format: Maintain the original format but optimize
   - JPEG: Convert to JPEG format (adjustable quality)
   - PNG: Convert to optimized PNG format
   - WebP: Convert to WebP format

3. **Adjust Settings**:
   - If JPEG format is selected, you can adjust the quality slider
   - If you need to rename files, check "Enable Renaming" and set related options

4. **Start Compression**:
   - Click the "Start Compression" button to begin processing
   - After processing, you can view the compression results for each file in the table

## Building the Executable

```bash
# Install PyInstaller
pip install pyinstaller

# Run the build script
python build.py
```

The generated executable will be located in the `dist` directory.

## Technical Details

- Built with PyQt5 for the graphical interface
- Supports PyQt-Fluent-Widgets style components
- Uses Pillow (PIL) for image processing
- PNG pipeline: imagequant + Floyd-Steinberg + oxipng (when available)
- JPEG pipeline: MozJPEG optimization (when available)
- Multi-threaded processing to keep the interface responsive

## Optional external capabilities

- `imagequant`: higher-ratio PNG quantization
- `mozjpeg-lossless-optimization`: JPEG lossless post-optimization
- `oxipng`: install in PATH or put binary at `bin/oxipng(.exe)`

## Contributing

Contributions of code, issue reports, or improvement suggestions are welcome! Please check the [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [PyQt5](https://www.riverbankcomputing.com/software/pyqt/)
- [Pillow](https://python-pillow.org/)
- [PyInstaller](https://www.pyinstaller.org/)

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=Moresl/ImageMinify&type=Date)](https://www.star-history.com/#Moresl/ImageMinify&Date)
