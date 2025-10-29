using System;
using System.Windows;

using Medoz.CatChast.Data;
using Medoz.CatChast.Services;

namespace Medoz.CatChast;

public class ApplicationCoordinator : IDisposable
{
    private readonly TrayManager _trayManager;
    private readonly IWindowService _windowService;
    private readonly IConfigService _configService;

    private readonly IServerService _serverService;

    public ApplicationCoordinator(
        IWindowService windowService,
        IConfigService configService,
        IServerService serverService)
    {
        _trayManager = new TrayManager();
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService), "WindowService cannot be null.");
        _configService = configService ?? throw new ArgumentNullException(nameof(configService), "ConfigService cannot be null.");
        _serverService = serverService ?? throw new ArgumentNullException(nameof(serverService), "ServerService cannot be null.");

        Initialize();
    }

    private void Initialize()
    {
        // イベントハンドラーを設定
        _trayManager.ShowMainWindowRequested += OnShowMainWindowRequested;
        _trayManager.ShowSettingsRequested += OnShowSettingsRequested;
        _trayManager.ExitRequested += OnExitRequested;

        // autorun
        AutoRun();
    }

    private void OnShowMainWindowRequested(object? sender, EventArgs e)
    {
        ToggleMainWindow();
    }

    private void OnShowSettingsRequested(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void ToggleMainWindow()
    {
        _windowService.ShowMainWindow();
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        // FIXME
    }

    private void ShowSettingsWindow()
    {
        _windowService.OpenSettingsWindow();
    }

    private void SettingsWindow_Closed(object? sender, EventArgs e)
    {
        _windowService.CloseSettingsWindow();
    }

    private void AutoRun()
    {
        var config = _configService.GetConfig();
        // Sub
        // FIXME

        // Pub
        // FIXME

        // WebAPI
        if (config.WebApiConfig.IsAutoStart)
        {
            // FIXME: ServerServiceがあまり実装されていない
            _serverService.StartWebApiAsync(config.WebApiConfig.Port);
        }
    }

    public void Dispose()
    {
        // FIXME
        _trayManager?.Dispose();
    }
}