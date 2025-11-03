using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MusicPlayer.Data;
using MusicPlayer.Models;
using Plugin.Maui.Audio;
using Microsoft.Maui.Dispatching;

namespace MusicPlayer.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly MusicDatabase _db;
        private readonly IAudioManager _audioManager;
        private IAudioPlayer _player;
        private Track _currentTrack;
        private bool _isPlaying;
        private double _progress;
        private string _positionText = "00:00";
        private string _durationText = "00:00";
        private string _kbpsText = "";
        private bool _isSeeking;
        private bool _progressTimerStarted;

        public ObservableCollection<Track> Tracks { get; set; } = new();

        public ICommand PlayCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PlayPauseCommand { get; }

        public MainViewModel(MusicDatabase database, IAudioManager audioManager)
        {
            _db = database;
            _audioManager = audioManager;

            PlayCommand = new Command<Track>(async (track) => await PlayTrack(track));
            DeleteCommand = new Command<Track>(async (track) => await DeleteTrack(track));
            PlayPauseCommand = new Command(TogglePlayPause);
        }

        // Properties
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (Math.Abs(_progress - value) > 0.001)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSeeking
        {
            get => _isSeeking;
            set => SetProperty(ref _isSeeking, value);
        }

        public string PositionText
        {
            get => _positionText;
            set => SetProperty(ref _positionText, value);
        }

        public string DurationText
        {
            get => _durationText;
            set => SetProperty(ref _durationText, value);
        }

        public string KbpsText
        {
            get => _kbpsText;
            set => SetProperty(ref _kbpsText, value);
        }

        public string CurrentTrackTitle => _currentTrack?.Title ?? "";

        // Load tracks from DB
        public async Task LoadTracksAsync()
        {
            Tracks.Clear();
            var items = await _db.GetTracksAsync();
            foreach (var i in items)
                Tracks.Add(i);
        }

        // Play selected track
        private async Task PlayTrack(Track track)
        {
            if (track == null) return;

            try
            {
                _player?.Stop();
                _player?.Dispose();
                _player = null;

                var stream = File.OpenRead(track.FilePath);
                _player = _audioManager.CreatePlayer(stream);

                _currentTrack = track;
                OnPropertyChanged(nameof(CurrentTrackTitle));

                _player.Play();
                IsPlaying = true;

                StartProgressTimer();
                UpdateRealBitrate(track);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Cannot play file:\n{ex.Message}", "OK");
            }
        }

        // Toggle Play/Pause
        private void TogglePlayPause()
        {
            if (_player == null) return;

            if (_player.IsPlaying)
            {
                _player.Pause();
                IsPlaying = false;
            }
            else
            {
                _player.Play();
                IsPlaying = true;
                StartProgressTimer();
            }
        }

        // Delete track
        private async Task DeleteTrack(Track track)
        {
            if (track == null) return;

            await _db.DeleteTrackAsync(track);
            Tracks.Remove(track);
        }

        // Update bitrate display
        private void UpdateRealBitrate(Track track)
        {
            try
            {
                var fi = new FileInfo(track.FilePath);
                var dur = _player?.Duration ?? 0;
                if (fi.Exists && dur > 0)
                {
                    var kbps = (int)Math.Round((fi.Length * 8.0) / dur / 1000.0);
                    KbpsText = $"{kbps} kbps";

                    if (kbps > 0 && track.Bitrate != kbps)
                    {
                        track.Bitrate = kbps;
                        _ = _db.SaveTrackAsync(track);
                    }
                }
                else
                {
                    KbpsText = track.Bitrate > 0 ? $"{track.Bitrate} kbps" : "";
                }
            }
            catch
            {
                KbpsText = track.Bitrate > 0 ? $"{track.Bitrate} kbps" : "";
            }
        }

        // Seek to fraction
        public void SeekToFraction(double fraction)
        {
            if (_player == null) return;

            var dur = _player.Duration;
            if (dur <= 0) return;

            var target = dur * Math.Clamp(fraction, 0, 1);
            _player.Seek(target);
            Progress = target / dur;
            OnPropertyChanged(nameof(Progress));
        }

        // Seek preview while dragging
        public void SeekPreview(double fraction)
        {
            if (_player == null || _player.Duration <= 0) return;

            var previewPosition = _player.Duration * Math.Clamp(fraction, 0, 1);
            PositionText = TimeSpan.FromSeconds(previewPosition).ToString(@"mm\:ss");
        }

        // Progress timer
        private void StartProgressTimer()
        {
            if (_progressTimerStarted) return;
            _progressTimerStarted = true;

            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (_player == null)
                {
                    _progressTimerStarted = false;
                    return false;
                }

                var pos = _player.CurrentPosition;
                var dur = _player.Duration;

                if (dur > 0)
                {
                    Progress = pos / dur;
                    PositionText = TimeSpan.FromSeconds(pos).ToString(@"mm\:ss");
                    DurationText = TimeSpan.FromSeconds(dur).ToString(@"mm\:ss");
                }

                return _player.IsPlaying;
            });
        }
    }
}
