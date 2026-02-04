using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MPTray.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static MPTray.MainWindow;

namespace MPTray.ViewModels
{
    public partial class MainVM : ViewModel
    {
        public PlayerVM PlayerVM { get; set; } 

        public SettingsVM SettingsVM { get; set; }

        public MainVM()
        {
            PlayerVM = new();
            SettingsVM = new();
        }

        [RelayCommand]
        public void OpenSettings()
        {
            WindowService.OpenSettingsWindow(SettingsVM);
        }

        [RelayCommand]
        public void OpenPlayer()
        {
            WindowService.OpenPlayerWindow(playerVM: PlayerVM, settingsVM: SettingsVM);
            PlayerVM.Run();
        }
    }
}
