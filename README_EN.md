# ImageMinify

![License](https://img.shields.io/github/license/moresl/ImageMinify)
![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![UI](https://img.shields.io/badge/UI-WPF%20%2B%20WPF--UI-blueviolet)

[中文](README.md) | English

A simple and efficient image compression tool built with C# WPF. Supports multiple formats, provides high-quality PNG/JPEG/WebP compression, and features batch processing and file renaming.

## Related Projects

This project has two versions for different use cases:

| Version | Description | Repository |
|---------|-------------|------------|
| **Desktop** | Current project, C# WPF + WPF-UI desktop application | [ImageMinify](https://github.com/Moresl/ImageMinify) |
| **Web** | React + FastAPI online image compression tool | [snapimg](https://github.com/Moresl/snapimg) |

## Features

- **Multiple Format Support**: Supports JPEG, PNG, WebP and other common image formats
- **Batch Processing**: Select an entire directory or specific image files for compression
- **Format Conversion**: Convert images to JPEG, PNG, or WebP format
- **PNG Quantization**: Supports imagequant algorithm with Floyd-Steinberg dithering
- **JPEG Optimization**: Supports MozJPEG lossless optimization (when available)
- **EXIF Metadata**: Supports reading and preserving image EXIF metadata
- **Fluent Design**: Modern Fluent Design UI powered by [WPF-UI](https://github.com/lepoco/wpfui)
- **File Renaming**: Custom prefixes, separators, and sequential numbering
- **Detailed Statistics**: Displays file sizes before and after compression with compression ratios
- **Persistent Settings**: Automatically saves user preferences

## Tech Stack

- **Framework**: .NET 10 + WPF
- **UI Library**: [WPF-UI](https://github.com/lepoco/wpfui) (Fluent Design)
- **Image Processing**: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- **EXIF Reading**: [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet)
- **MVVM**: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- **Native Compression**: imagequant (optional), MozJPEG (optional)

## Screenshot

![image](https://github.com/user-attachments/assets/a11d90d1-fff5-461e-aad1-2ea72c08778d)

> Screenshot shows the legacy Python UI. C# version screenshot coming soon.

## Installation

### Method 1: Download Executable

1. Download the latest version from the [Releases](https://github.com/Moresl/ImageMinify/releases) page
2. Extract the downloaded file
3. Double-click `ImageMinify.exe` to run

### Method 2: Build from Source

**Prerequisites**:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
# Clone the repository
git clone https://github.com/Moresl/ImageMinify
cd ImageMinify

# Build the project
dotnet build -c Release

# Run the application
dotnet run -c Release
```

## Usage

1. **Select Files or Directory**:
   - Click "Select Directory" to choose a folder containing images
   - Or click "Select Files" to choose one or more image files

2. **Choose Output Format**:
   - Keep Original: Maintain original format with optimization
   - JPEG: Convert to JPEG (adjustable quality)
   - PNG: Convert to optimized PNG
   - WebP: Convert to WebP format

3. **Adjust Settings**:
   - Adjust the compression quality slider
   - Enable file renaming if needed and configure options

4. **Start Compression**:
   - Click "Start Compression" to begin processing
   - View compression results for each file in the table

## Publishing

```bash
# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained true
```

Output is located in `bin/Release/net10.0-windows/win-x64/publish/`.

## Project Structure

```
ImageMinify/
├── App.xaml(.cs)              # Application entry
├── GlobalUsings.cs            # Global using declarations
├── ImageMinify.csproj         # Project file
├── Models/                    # Data models
│   ├── CompressionResult.cs   # Compression result
│   ├── CompressionSummary.cs  # Compression statistics
│   ├── EngineCapabilities.cs  # Engine capabilities
│   └── RenameSettings.cs      # Rename settings
├── Services/                  # Business services
│   ├── ImageCompressor.cs     # Image compression (main)
│   ├── PngCompressor.cs       # PNG compressor
│   ├── JpegCompressor.cs      # JPEG compressor
│   ├── WebpCompressor.cs      # WebP compressor
│   ├── ExifService.cs         # EXIF metadata
│   ├── FileRenameService.cs   # File renaming
│   └── SettingsService.cs     # Settings persistence
├── ViewModels/                # View models (MVVM)
│   └── MainViewModel.cs       # Main view model
├── Views/                     # Views
│   └── MainWindow.xaml(.cs)   # Main window
├── Helpers/                   # Utilities
└── tests/                     # Unit tests
```

## Contributing

Contributions are welcome! Please see the [Contributing Guide](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) - Cross-platform image processing
- [WPF-UI](https://github.com/lepoco/wpfui) - Fluent Design WPF control library
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit
- [MetadataExtractor](https://github.com/drewnoakes/metadata-extractor-dotnet) - Image metadata reading
