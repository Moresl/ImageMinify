using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMinify.Models;
using ImageMinify.Services;

namespace ImageMinify.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IImageCompressor _imageCompressor;
    private readonly FileRenameService _fileRenameService;
    private readonly SettingsService _settingsService;
    private readonly List<string> _selectedFiles = [];
    private bool _isInitializing;
    private string? _selectedDirectoryPath;

    public MainViewModel(
        IImageCompressor imageCompressor,
        FileRenameService fileRenameService,
        SettingsService settingsService)
    {
        _imageCompressor = imageCompressor;
        _fileRenameService = fileRenameService;
        _settingsService = settingsService;

        Summary = new CompressionSummary();
        EngineCapabilities = _imageCompressor.GetCapabilities();

        SelectDirectoryCommand = new AsyncRelayCommand(SelectDirectoryAsync, () => !IsCompressing);
        SelectFilesCommand = new AsyncRelayCommand(SelectFilesAsync, () => !IsCompressing);
        StartCompressionCommand = new AsyncRelayCommand(StartCompressionAsync, CanStartCompression);
    }

    public ObservableCollection<CompressionResult> Results { get; } = [];

    public Func<Task<string?>>? SelectDirectoryHandler { get; set; }

    public Func<Task<IReadOnlyList<string>>>? SelectFilesHandler { get; set; }

    [ObservableProperty]
    private string _selectedDirectoryDisplay = "未选择目录";

    [ObservableProperty]
    private string _selectedFilesDisplay = "未选择文件";

    [ObservableProperty]
    private string _outputFormat = "original";

    [ObservableProperty]
    private int _quality = 85;

    [ObservableProperty]
    private bool _isQualityEnabled;

    [ObservableProperty]
    private bool _isRenameEnabled;

    [ObservableProperty]
    private string _renamePrefix = string.Empty;

    [ObservableProperty]
    private string _renameSeparator = "-";

    [ObservableProperty]
    private int _startNumber = 1;

    [ObservableProperty]
    private bool _isCompressing;

    [ObservableProperty]
    private int _progressValue;

    [ObservableProperty]
    private bool _isProgressVisible;

    [ObservableProperty]
    private CompressionSummary _summary;

    [ObservableProperty]
    private EngineCapabilities _engineCapabilities;

    [ObservableProperty]
    private bool _isStatusOpen;

    [ObservableProperty]
    private string _statusTitle = "状态";

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private StatusSeverity _statusSeverity = StatusSeverity.Informational;

    public AsyncRelayCommand SelectDirectoryCommand { get; }

    public AsyncRelayCommand SelectFilesCommand { get; }

    public AsyncRelayCommand StartCompressionCommand { get; }

    public async Task InitializeAsync()
    {
        _isInitializing = true;

        var snapshot = await _settingsService.LoadAsync();
        OutputFormat = snapshot.OutputFormat;
        Quality = snapshot.Quality;
        IsRenameEnabled = snapshot.RenameEnabled;
        RenamePrefix = snapshot.RenamePrefix;
        RenameSeparator = snapshot.RenameSeparator;
        StartNumber = snapshot.RenameStartNumber;
        EngineCapabilities = _imageCompressor.GetCapabilities();

        _selectedDirectoryPath = Directory.Exists(snapshot.LastDirectory) ? snapshot.LastDirectory : null;
        _selectedFiles.Clear();
        _selectedFiles.AddRange(snapshot.LastFiles.Where(File.Exists));

        if (snapshot.LastMode == "files" && _selectedFiles.Count > 0)
        {
            SelectedDirectoryDisplay = "未选择目录";
            SelectedFilesDisplay = _selectedFiles.Count == 1
                ? Path.GetFileName(_selectedFiles[0])
                : $"已选择 {_selectedFiles.Count} 个文件";
            _selectedDirectoryPath = null;
        }
        else if (snapshot.LastMode == "directory" && !string.IsNullOrWhiteSpace(_selectedDirectoryPath))
        {
            SelectedDirectoryDisplay = _selectedDirectoryPath;
            SelectedFilesDisplay = "未选择文件";
            _selectedFiles.Clear();
        }
        else
        {
            SelectedDirectoryDisplay = "未选择目录";
            SelectedFilesDisplay = "未选择文件";
            _selectedDirectoryPath = null;
            _selectedFiles.Clear();
        }

        IsQualityEnabled = OutputFormat == "jpeg";
        Summary = new CompressionSummary();

        _isInitializing = false;
        RefreshCommandStates();
    }

    private bool CanStartCompression()
    {
        return !IsCompressing && (!string.IsNullOrWhiteSpace(_selectedDirectoryPath) || _selectedFiles.Count > 0);
    }

    private async Task SelectDirectoryAsync()
    {
        if (SelectDirectoryHandler is null)
        {
            return;
        }

        var directory = await SelectDirectoryHandler();
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        _selectedDirectoryPath = directory;
        _selectedFiles.Clear();

        SelectedDirectoryDisplay = directory;
        SelectedFilesDisplay = "未选择文件";
        await PersistSettingsAsync();
        RefreshCommandStates();
    }

    private async Task SelectFilesAsync()
    {
        if (SelectFilesHandler is null)
        {
            return;
        }

        var files = await SelectFilesHandler();
        if (files.Count == 0)
        {
            return;
        }

        _selectedDirectoryPath = null;
        _selectedFiles.Clear();
        _selectedFiles.AddRange(files.Where(File.Exists));

        SelectedDirectoryDisplay = "未选择目录";
        SelectedFilesDisplay = _selectedFiles.Count == 1
            ? Path.GetFileName(_selectedFiles[0])
            : $"已选择 {_selectedFiles.Count} 个文件";
        await PersistSettingsAsync();
        RefreshCommandStates();
    }

    private async Task StartCompressionAsync()
    {
        if (!CanStartCompression())
        {
            OpenStatus("警告", "请先选择目录或文件。", StatusSeverity.Warning);
            return;
        }

        Results.Clear();
        Summary = new CompressionSummary();
        ProgressValue = 0;
        IsProgressVisible = true;
        IsCompressing = true;
        IsStatusOpen = false;
        RefreshCommandStates();

        try
        {
            if (!string.IsNullOrWhiteSpace(_selectedDirectoryPath))
            {
                await CompressDirectorySelectionAsync(_selectedDirectoryPath);
            }
            else
            {
                await CompressFileSelectionAsync();
            }

            OpenStatus(
                "压缩完成",
                $"已压缩 {Summary.ProcessedFiles} 个图片文件，总大小从 {Summary.TotalOriginalSizeFormatted} 减小到 {Summary.TotalCompressedSizeFormatted} ({Summary.OverallCompressionRatio:F2}%)。",
                StatusSeverity.Success);
        }
        catch (Exception ex)
        {
            OpenStatus("压缩失败", ex.Message, StatusSeverity.Error);
        }
        finally
        {
            IsCompressing = false;
            IsProgressVisible = false;
            RefreshCommandStates();
            await PersistSettingsAsync();
        }
    }

    private async Task CompressDirectorySelectionAsync(string directoryPath)
    {
        var progress = new Progress<(int Processed, int Total, CompressionResult? Result)>(progressUpdate =>
        {
            ProgressValue = progressUpdate.Total == 0
                ? 0
                : (int)Math.Round(progressUpdate.Processed * 100D / progressUpdate.Total);

            if (progressUpdate.Result is { Success: true } result)
            {
                var finalResult = ApplyRenameIfNeeded(result, progressUpdate.Processed, progressUpdate.Total);
                Results.Add(finalResult);
            }
        });

        var (_, summary) = await _imageCompressor.CompressDirectoryAsync(
            directoryPath,
            OutputFormat,
            Quality,
            progress);

        Summary = summary with { ProcessedFiles = Results.Count };
    }

    private async Task CompressFileSelectionAsync()
    {
        var successfulResults = new List<CompressionResult>();
        var totalFiles = _selectedFiles.Count;

        for (var index = 0; index < totalFiles; index++)
        {
            var result = await _imageCompressor.CompressAsync(
                _selectedFiles[index],
                outputFormat: OutputFormat,
                quality: Quality);

            ProgressValue = totalFiles == 0 ? 0 : (int)Math.Round((index + 1) * 100D / totalFiles);

            if (!result.Success)
            {
                continue;
            }

            var finalResult = ApplyRenameIfNeeded(result, index + 1, totalFiles);
            Results.Add(finalResult);
            successfulResults.Add(finalResult);
        }

        Summary = CompressionSummary.FromResults(totalFiles, successfulResults.Count, successfulResults);
    }

    private CompressionResult ApplyRenameIfNeeded(CompressionResult result, int processed, int total)
    {
        if (!IsRenameEnabled)
        {
            return result;
        }

        var settings = new RenameSettings
        {
            Enabled = true,
            Prefix = RenamePrefix,
            Separator = RenameSeparator,
            StartNumber = StartNumber,
        };

        return _fileRenameService.ApplyRename(result, settings, processed, total);
    }

    private void OpenStatus(string title, string message, StatusSeverity severity)
    {
        StatusTitle = title;
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = true;
    }

    private async Task PersistSettingsAsync()
    {
        if (_isInitializing)
        {
            return;
        }

        await _settingsService.SaveAsync(new AppSettingsSnapshot
        {
            Quality = Quality,
            OutputFormat = OutputFormat,
            RenameEnabled = IsRenameEnabled,
            RenamePrefix = RenamePrefix,
            RenameSeparator = RenameSeparator,
            RenameStartNumber = StartNumber,
            LastDirectory = _selectedDirectoryPath ?? string.Empty,
            LastFiles = _selectedFiles.ToList(),
            LastMode = _selectedFiles.Count > 0 ? "files" : (!string.IsNullOrWhiteSpace(_selectedDirectoryPath) ? "directory" : string.Empty),
        });
    }

    private void RefreshCommandStates()
    {
        SelectDirectoryCommand.NotifyCanExecuteChanged();
        SelectFilesCommand.NotifyCanExecuteChanged();
        StartCompressionCommand.NotifyCanExecuteChanged();
    }

    partial void OnOutputFormatChanged(string value)
    {
        IsQualityEnabled = value == "jpeg";
        _ = PersistSettingsAsync();
    }

    partial void OnQualityChanged(int value)
    {
        _ = PersistSettingsAsync();
    }

    partial void OnIsRenameEnabledChanged(bool value)
    {
        _ = PersistSettingsAsync();
    }

    partial void OnRenamePrefixChanged(string value)
    {
        _ = PersistSettingsAsync();
    }

    partial void OnRenameSeparatorChanged(string value)
    {
        _ = PersistSettingsAsync();
    }

    partial void OnStartNumberChanged(int value)
    {
        _ = PersistSettingsAsync();
    }

    partial void OnIsCompressingChanged(bool value)
    {
        RefreshCommandStates();
    }
}
