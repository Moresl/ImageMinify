using System.Windows;
using ImageMinify.Services;
using ImageMinify.ViewModels;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ImageMinify.Views;

public partial class MainWindow : FluentWindow
{
    private readonly MainViewModel _viewModel;
    private readonly SettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();

        var exifService = new ExifService();
        _settingsService = new SettingsService();
        var imageCompressor = new ImageCompressor(
            new JpegCompressor(exifService),
            new PngCompressor(exifService),
            new WebpCompressor(exifService));

        _viewModel = new MainViewModel(imageCompressor, new FileRenameService(), _settingsService)
        {
            SelectDirectoryHandler = PickDirectoryAsync,
            SelectFilesHandler = PickFilesAsync,
        };

        DataContext = _viewModel;

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _settingsService.RestoreWindowPlacement(this);
        await _viewModel.InitializeAsync();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _settingsService.SaveWindowPlacement(this);
    }

    private Task<string?> PickDirectoryAsync()
    {
        var dialog = new OpenFolderDialog
        {
            Multiselect = false,
            Title = "选择图片目录",
        };

        return Task.FromResult(dialog.ShowDialog() == true ? dialog.FolderName : null);
    }

    private Task<IReadOnlyList<string>> PickFilesAsync()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Title = "选择图片文件",
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.webp|所有文件|*.*",
        };

        IReadOnlyList<string> files = dialog.ShowDialog() == true ? dialog.FileNames : [];
        return Task.FromResult(files);
    }
}
