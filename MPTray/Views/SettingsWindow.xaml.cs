using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MPTray.ViewModels;
using MPTray.Views;
using Windows.UI;

namespace MPTray;

public sealed partial class SettingsWindow : Window
{
    public SettingsVM SettingsVM;

    public SettingsWindow(SettingsVM settingsVM)
    {
        SettingsVM = settingsVM;
        InitializeComponent();
        SettingsVM.IsSettingsOpened = true;
        Closed += SettingsWindow_Closed;
        ExtendsContentIntoTitleBar = true;
        if (Application.Current?.RequestedTheme == ApplicationTheme.Light)
            return;
        var titleBar = AppWindow.TitleBar;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        titleBar.ButtonForegroundColor = Colors.White;
        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(25, 255, 255, 255);
        titleBar.ButtonHoverForegroundColor = Colors.White;
    }

    private void SettingsWindow_Closed(object sender, WindowEventArgs args)
    {
        SettingsVM.IsSettingsOpened = false;
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var tag = (args.SelectedItem as NavigationViewItem)?.Tag?.ToString();
        PlayerSection.Visibility = (tag == "player") ? Visibility.Visible : Visibility.Collapsed;
        AppSection.Visibility = (tag == "app") ? Visibility.Visible : Visibility.Collapsed;
    }
}
