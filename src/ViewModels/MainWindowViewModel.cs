using ARM9Editor.Services;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
namespace ARM9Editor;

public partial class MainWindowViewModel : ObservableObject
{
    private const string DefaultTitle = "Mario Kart DS ARM9 Editor";
    private const string RepoUrl = "https://github.com/LandonAndEmma/MKDS-ARM9-Editor";

    [ObservableProperty]
    private string _status = "Ready";

    [ObservableProperty]
    private string _title = DefaultTitle;

    private string? _filePath;

    public Window? Owner { get; set; }

    public IRelayCommand? OpenFileCommand { get; }
    public IRelayCommand? SaveFileCommand { get; }
    public IRelayCommand? SaveFileAsCommand { get; }
    public IRelayCommand? ExportChangesCommand { get; }
    public IRelayCommand? ImportChangesCommand { get; }
    public IRelayCommand? ShowInfoCommand { get; }
    public IRelayCommand? OpenRepositoryCommand { get; }

    public ARM9Data Data { get; } = new();
    [ObservableProperty]
    private bool _isFileLoaded;

    public MainWindowViewModel()
    {
        OpenFileCommand = new RelayCommand(async () => await OpenFileAsync());
        SaveFileCommand = new RelayCommand(async () => await SaveFileAsync(), () => IsFileLoaded);
        SaveFileAsCommand = new RelayCommand(async () => await SaveFileAsAsync(), () => IsFileLoaded);
        ExportChangesCommand = new RelayCommand(async () => await ExportChangesAsync(), () => IsFileLoaded && Data.HasChanges());
        ImportChangesCommand = new RelayCommand(async () => await ImportChangesAsync(), () => IsFileLoaded);
        ShowInfoCommand = new RelayCommand(async () => await ShowInfoAsync());
        OpenRepositoryCommand = new RelayCommand(async () => await OpenRepositoryAsync());
        Data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ARM9Data.IsLoaded))
            {
                IsFileLoaded = Data.IsLoaded;
            }
        };
    }

    public async Task OpenFileAsync()
    {
        try
        {
            (byte[]? data, string? path) = await FileService.OpenFileAsync(Owner);
            if (data == null || path == null)
            {
                return;
            }
            Data.Load(data);
            _filePath = path;
            string fileName = Path.GetFileName(_filePath);
            Title = $"{DefaultTitle} - {fileName}";
            Status = $"Loaded: {fileName}";
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, ex.Message);
        }
    }

    public async Task SaveFileAsync()
    {
        if (!IsFileLoaded)
        {
            await DialogService.ShowErrorAsync(Owner, "No file loaded.");
            return;
        }
        if (string.IsNullOrEmpty(_filePath))
        {
            await SaveFileAsAsync();
            return;
        }
        try
        {
            await FileService.SaveFileAsync(_filePath, Data.ToArray());
            Status = $"Saved: {Path.GetFileName(_filePath)}";
            await DialogService.ShowMessageAsync(Owner, "Success", "File saved successfully.");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, ex.Message);
        }
    }

    public async Task SaveFileAsAsync()
    {
        if (!IsFileLoaded)
        {
            await DialogService.ShowErrorAsync(Owner, "No file loaded.");
            return;
        }
        try
        {
            string? path = await FileService.SaveFileAsAsync(Owner);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            await FileService.SaveFileAsync(path, Data.ToArray());
            _filePath = path;
            string fileName = Path.GetFileName(_filePath);
            Title = $"{DefaultTitle} - {fileName}";
            Status = $"Saved: {fileName}";
            await DialogService.ShowMessageAsync(Owner, "Success", "File saved successfully.");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, ex.Message);
        }
    }

    [RequiresUnreferencedCode("Calls FileService.ExportChangesAsync")]
    public async Task ExportChangesAsync()
    {
        if (!IsFileLoaded)
        {
            await DialogService.ShowErrorAsync(Owner, "No file loaded.");
            return;
        }
        if (!Data.HasChanges())
        {
            await DialogService.ShowMessageAsync(Owner, "No Changes", "No changes have been made to export.");
            return;
        }
        try
        {
            ChangesExport changes = Data.ExportChanges();
            string? path = await FileService.ExportChangesAsync(Owner, changes);
            if (!string.IsNullOrEmpty(path))
            {
                Status = $"Exported {changes.Changes.Count} change(s) to: {Path.GetFileName(path)}";
                await DialogService.ShowMessageAsync(Owner, "Success", $"Successfully exported {changes.Changes.Count} change(s) to JSON.");
            }
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, ex.Message);
        }
    }

    [RequiresUnreferencedCode("Calls FileService.ImportChangesAsync")]
    public async Task ImportChangesAsync()
    {
        if (!IsFileLoaded)
        {
            await DialogService.ShowErrorAsync(Owner, "No file loaded.");
            return;
        }
        try
        {
            ChangesExport? changes = await FileService.ImportChangesAsync(Owner);
            if (changes == null)
            {
                return;
            }
            Data.ImportChanges(changes);
            Status = $"Imported {changes.Changes.Count} change(s)";
            await DialogService.ShowMessageAsync(Owner, "Success", $"Successfully imported {changes.Changes.Count} change(s).");
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, ex.Message);
        }
    }

    public async Task ShowInfoAsync()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? version = assembly.GetName().Version;
        string versionStr = version != null
            ? $"{version.Major}.{version.Minor}.{Math.Max(0, version.Build)}"
            : "Unknown";
        AssemblyCompanyAttribute? companyAttr = assembly.GetCustomAttribute<AssemblyCompanyAttribute>();
        string message = $"Mario Kart DS ARM9 Editor\nVersion: {versionStr}\n\nEdit values in Mario Kart DS ARM9 files.\n\n";
        if (companyAttr != null)
        {
            message += $"By: {companyAttr.Company}\n";
        }
        message += "Special Thanks: Ermelber, Yami, MkDasher";
        await DialogService.ShowMessageAsync(Owner, "Info", message);
    }

    public async Task OpenRepositoryAsync()
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = RepoUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            await DialogService.ShowErrorAsync(Owner, $"Cannot open URL: {ex.Message}");
        }
    }
}