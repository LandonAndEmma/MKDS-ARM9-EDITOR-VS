using ARM9Editor.Services;
using ARM9Editor.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
namespace ARM9Editor;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _viewModel;
    private MainWindowViewModel ViewModel
    {
        get
        {
            if (_viewModel != null)
            {
                return _viewModel;
            }
            if (DataContext is MainWindowViewModel vm)
            {
                _viewModel = vm;
                return vm;
            }
            return null!;
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel != null)
        {
            _viewModel.Owner = this;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.IsFileLoaded))
        {
            UpdateViewVisibility();
        }
    }

    private void UpdateViewVisibility()
    {
        Border? welcomePanel = this.FindControl<Border>("WelcomePanel");
        TabControl? tabControl = this.FindControl<TabControl>("MainTabControl");
        if (ViewModel == null)
        {
            if (welcomePanel != null) welcomePanel.IsVisible = true;
            if (tabControl != null) tabControl.IsVisible = false;
            return;
        }
        if (welcomePanel != null) welcomePanel.IsVisible = !ViewModel.IsFileLoaded;
        if (tabControl != null) tabControl.IsVisible = ViewModel.IsFileLoaded;
        if (ViewModel.IsFileLoaded)
        {
            PopulateTabs();
        }
    }

    private void PopulateTabs()
    {
        if (ViewModel == null || !ViewModel.IsFileLoaded)
        {
            return;
        }
        TabControl? tabControl = this.FindControl<TabControl>("MainTabControl");
        if (tabControl == null)
        {
            return;
        }
        tabControl.Items.Clear();
        foreach (EditorTab tabType in Enum.GetValues<EditorTab>())
        {
            TabConfig config = ConfigurationService.Instance.GetTabConfig(tabType);
            TabItem tabItem = new()
            {
                Header = config.Header,
                Content = new EditorContent(tabType, ViewModel)
            };
            _ = tabControl.Items.Add(tabItem);
        }
        tabControl.SelectedIndex = 0;
    }

    private void RefreshEditorContent()
    {
        TabControl? tabControl = this.FindControl<TabControl>("MainTabControl");
        if (tabControl == null)
        {
            return;
        }
        foreach (object? item in tabControl.Items)
        {
            if (item is TabItem tabItem && tabItem.Content is EditorContent content)
            {
                content.Refresh();
            }
        }
    }

    private async void OnOpenClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.OpenFileAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.SaveFileAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    private async void OnSaveAsClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.SaveFileAsAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    [RequiresUnreferencedCode("Calls ARM9Editor.MainWindowViewModel.ExportChangesAsync()")]
    private async void OnExportChangesClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.ExportChangesAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    [RequiresUnreferencedCode("Calls ARM9Editor.MainWindowViewModel.ImportChangesAsync()")]
    private async void OnImportChangesClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.ImportChangesAsync();
                RefreshEditorContent();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    private async void OnInfoClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.ShowInfoAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    private async void OnRepositoryClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
        {
            try
            {
                await ViewModel.OpenRepositoryAsync();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(ViewModel.Owner, ex.Message);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel?.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnClosed(e);
    }
}