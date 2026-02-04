using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using MPTray.ViewModels;
using System;
using System.Runtime.InteropServices;
using Windows.Graphics;

namespace MPTray.Views;

public sealed partial class PlayerWindow : Window
{
    public PlayerVM PlayerVM;

    public SettingsVM SettingsVM;

    private PointInt32 _point;
    
    public PlayerWindow(PlayerVM playerVM, SettingsVM settingsVM, PointInt32 point)
    {
        PlayerVM = playerVM;
        SettingsVM = settingsVM;
        _point = point;
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(400, 150));
        if (AppWindow.Presenter is not OverlappedPresenter presenter)
            return;
        ExtendsContentIntoTitleBar = true;
        presenter.IsAlwaysOnTop = true;
        presenter.IsResizable = false;
        presenter.IsMaximizable = false;
        presenter.IsMinimizable = false;
        presenter.SetBorderAndTitleBar(true, false);
        AppWindow.Closing += AppWindow_Closing;
        ProgressSlider.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnSliderPointerPressed), true);
        ProgressSlider.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnSliderPointerReleased), true);
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
            Close();
        else
        {
            var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);
            var workArea = displayArea.WorkArea;
            int playerHeight = AppWindow.Size.Height;
            int playerWidth = AppWindow.Size.Width;
            int x = _point.X - playerWidth / 2;
            int y = workArea.Y + workArea.Height - playerHeight;
            AppWindow.Move(new PointInt32(x, y));
        }    
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        args.Cancel = true; // cancel alt+F4
    }

    private void OnSliderPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        PlayerVM.StartSeekCommand.Execute(null);
    }

    private void OnSliderPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        PlayerVM.EndSeekCommand.Execute(ProgressSlider.Value);
    }
}
