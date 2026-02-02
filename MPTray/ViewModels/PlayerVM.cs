using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using MPTray.Services;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Control;

namespace MPTray.ViewModels
{
    public partial class PlayerVM : ViewModel
    {
        private DispatcherQueue _dispatcher { get; } = DispatcherQueue.GetForCurrentThread();

        private CancellationTokenSource? _updateCts;

        private bool _isSeeking;

        private DateTimeOffset _seekTimestamp;

        private DateTime _lastUserActionTime = DateTime.MinValue;

        private DateTimeOffset _lastSystemTimelineUpdate = DateTimeOffset.MinValue;

        private string _source = string.Empty;

        public string Source
        {
            get => _source;
            set => Set(ref _source, value);
        }

        private string _title = "No media";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _artist = "MPTray";

        public string Artist
        {
            get => _artist;
            set => Set(ref _artist, value);
        }

        private TimeSpan _duration;

        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (!Set(ref _duration, value))
                    return;
                OnPropertyChanged(nameof(DurationText));
                OnPropertyChanged(nameof(DurationSeconds));
            }
        }

        private TimeSpan _position;

        public TimeSpan Position
        {
            get => _position;
            set
            {
                if (!Set(ref _position, value))
                    return;
                OnPropertyChanged(nameof(PositionText));
                OnPropertyChanged(nameof(PositionSeconds));
            }
        }

        public string PositionText
        {
            get
            {
                if (Duration.TotalHours >= 1)
                    return Position.ToString(@"hh\:mm\:ss");
                return Position.ToString(@"mm\:ss");
            }
        }

        public string DurationText
        {
            get
            {
                if (Duration.TotalHours >= 1)
                    return Duration.ToString(@"hh\:mm\:ss");
                return Duration.ToString(@"mm\:ss");
            }
        }

        public double PositionSeconds => Position.TotalSeconds;

        public double DurationSeconds => Duration.TotalSeconds; 

        /*private GlobalSystemMediaTransportControlsSessionPlaybackStatus _status;

        public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status
        {
            get => _status;
            set => Set(ref _status, value);
        }*/

        private bool _isPlaying;

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
        }

        private ImageSource _thumbnailSource;

        public ImageSource ThumbnailSource
        {
            get => _thumbnailSource;
            set => Set(ref _thumbnailSource, value);
        }

        [RelayCommand]
        public void OpenPlayer()
        {
            WindowService.OpenPlayerWindow(playerVM: this);
            _updateCts?.Cancel();
            _updateCts = new CancellationTokenSource();
            _ = UpdatingAsync(_updateCts.Token);
        }

        [RelayCommand]
        public void StartSeek()
        {
            _isSeeking = true;
        }

        [RelayCommand]
        public async Task EndSeek(double seconds)
        {
            _seekTimestamp = DateTimeOffset.UtcNow;
            _lastSystemTimelineUpdate = DateTimeOffset.MinValue;
            try
            {
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();
                await session?.TryChangePlaybackPositionAsync((long)TimeSpan.FromSeconds(seconds).Ticks);
                Position = TimeSpan.FromSeconds(seconds);
            }
            catch { }
            _isSeeking = false;
        }

        [RelayCommand]
        public void ChangePosition(double seconds)
        {
            Position = TimeSpan.FromSeconds(seconds);
        }

        [RelayCommand]
        public async Task Back()
        {
            try
            {
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();
                await session?.TrySkipPreviousAsync();
            }
            catch { }
        }

        [RelayCommand]
        public async Task Pause()
        {
            _lastUserActionTime = DateTime.UtcNow;
            IsPlaying = !IsPlaying;
            try
            {
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();
                var timeline = session.GetTimelineProperties();
                Position = timeline.Position;
                if (!IsPlaying)
                    await session?.TryPauseAsync();
                else
                    await session?.TryPlayAsync();
            }
            catch
            {
                IsPlaying = !IsPlaying;
            }
        }

        [RelayCommand]
        public async Task Next()
        {
            try
            {
                var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                var session = manager.GetCurrentSession();
                await session?.TrySkipNextAsync();
            }
            catch { }
        }

        private async void GetTrackInfo(GlobalSystemMediaTransportControlsSession session)
        {
            try
            {
                var mediaProperties = await session.TryGetMediaPropertiesAsync().AsTask();
                if (mediaProperties == null)
                    return;
                Source = session.SourceAppUserModelId;
                Title = mediaProperties.Title;
                Artist = mediaProperties.Artist;
                if (mediaProperties.Thumbnail != null)
                {
                    try
                    {
                        using (var stream = await mediaProperties.Thumbnail.OpenReadAsync())
                        {
                            var bitmap = new BitmapImage();
                            await bitmap.SetSourceAsync(stream);
                            ThumbnailSource = bitmap;
                        }
                    }
                    catch { }
                }
                var playbackInfo = session.GetPlaybackInfo();
                if (playbackInfo is null)
                    return;
                var status = playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                if (DateTime.UtcNow - _lastUserActionTime > TimeSpan.FromSeconds(0.5))
                {
                    if (IsPlaying != status)
                        IsPlaying = status;
                }
            }
            catch { }
        }

        private async void GetTimeUpdate(GlobalSystemMediaTransportControlsSession session)
        {
            var timeline = session.GetTimelineProperties();
            if (timeline == null || _isSeeking || timeline.LastUpdatedTime < _seekTimestamp) 
                return;
            Duration = timeline.MaxSeekTime;
            bool isNewSystemData = timeline.LastUpdatedTime != _lastSystemTimelineUpdate;
            if (isNewSystemData)
                _lastSystemTimelineUpdate = timeline.LastUpdatedTime;
            if (IsPlaying)
            {
                var timeSinceUpdate = DateTimeOffset.UtcNow - timeline.LastUpdatedTime;
                var predicted = timeline.Position + timeSinceUpdate;
                if (predicted < Duration)
                    Position = predicted;
            }
            else if (isNewSystemData)
                Position = timeline.Position;
        }

        private async Task UpdatingAsync(CancellationToken token)
        {
            var manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            var session = manager.GetCurrentSession();
            if (session is null)
                return;
            session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
            session.PlaybackInfoChanged += Session_PlaybackInfoChanged;
            session.TimelinePropertiesChanged += Session_TimelinePropertiesChanged;
            token.Register(() =>
            {
                session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
                session.TimelinePropertiesChanged -= Session_TimelinePropertiesChanged;
            });
            GetTrackInfo(session);
            while (!token.IsCancellationRequested)
            {
                if (session != null)
                    GetTimeUpdate(session);
                else
                    Title = "Media not found";
                await Task.Delay(100, token);
            }
        }

        private void Session_TimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            var timeline = sender.GetTimelineProperties();
            Position = timeline.Position;
        }

        private void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            _dispatcher.TryEnqueue(() =>
            {
                if (sender == null) 
                    return;
                var info = sender.GetPlaybackInfo();
                if (info != null)
                {
                    var status = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                    if (DateTime.UtcNow - _lastUserActionTime > TimeSpan.FromSeconds(1.0))
                        IsPlaying = status;
                }
            });
        }

        private void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            _dispatcher.TryEnqueue(async () =>
            {
                GetTrackInfo(sender);
                var target = TimeSpan.FromSeconds(0);
                await sender.TryChangePlaybackPositionAsync((long)target.Ticks);
            });
        }
    }
}
