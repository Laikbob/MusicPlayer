using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using MusicPlayer.Resources.Localization;

namespace MusicPlayer.Helpers
{
    public class BoolToPlayPauseTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlaying = value is bool b && b;
            // Используем переводы из AppResources вместо "Play"/"Pause"
            return isPlaying ? AppResources.PauseText : AppResources.PlayText;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
