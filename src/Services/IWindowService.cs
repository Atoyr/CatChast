
using System.Reactive.Disposables;

using Medoz.CatChast.Clients;
using Medoz.CatChast.Data;

namespace Medoz.CatChast.Services;

public interface IWindowService
{
    void OpenMainWindow();
    void CloseMainWindow();
    void ShowMainWindow();
    void HideMainWindow();

    void SetMainWindowSize(double width, double height, bool save = true);
    void SetMainWindowPosition(double x, double y, bool save = true);

    void SaveMainWindowSize();
    void SaveMainWindowPosition();

    bool IsMainWindowVisible();
    void MainWindowMessageClear();

    void ToggleMoveableWindow();
    void ChangeMoveableWindowState(bool isMoveable);

    void SetMainWindowSize(double width, double height);
    void SetMainWindowPosition(double x, double y);

    void OpenSettingsWindow();
    void CloseSettingsWindow();

    event EventHandler? OpenMainWindowRequested;
    event EventHandler? CloseMainWindowRequested;
    event EventHandler? WindowSizeChanged;
    event EventHandler<bool>? MoveableWindowStateChanged;

    event EventHandler? OpenSettingWindowRequested;
    event EventHandler? CloseSettingWindowRequested;
}
