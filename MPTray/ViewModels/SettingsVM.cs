using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage;
using MPTray.Models;
using MPTray.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MPTray.ViewModels
{
    public partial class SettingsVM : ViewModel
    {
        private readonly PlayerSettings _playerSettings = SettingsService.Load();

        public bool IsSettingsOpened { get; set; }

        public SettingsVM()
        {
            _playerStyle = (PlayerStyle)_playerSettings.PlayerStyle;
            _isSliderOn = _playerSettings.IsSliderOn;
            _isCoverBorderOn = _playerSettings.IsCoverBorderOn;
            _isCoverShadowOn = _playerSettings.IsCoverShadowOn;
        }
        
        private PlayerStyle _playerStyle;

        public PlayerStyle PlayerStyle
        {
            get => _playerStyle;
            set => Set(ref _playerStyle, value);
        }

        private bool _isSliderOn;

        public bool IsSliderOn
        {
            get => _isSliderOn;
            set
            {
                Set(ref _isSliderOn, value);
                _playerSettings.IsSliderOn = value;
                SettingsService.Save(_playerSettings);
            }
        }

        private bool _isCoverBorderOn;

        public bool IsCoverBorderOn
        {
            get => _isCoverBorderOn;
            set => Set(ref _isCoverBorderOn, value);
        }

        private bool _isCoverShadowOn;

        public bool IsCoverShadowOn
        {
            get => _isCoverShadowOn;
            set => Set(ref _isCoverShadowOn, value);
        }


    }
}
