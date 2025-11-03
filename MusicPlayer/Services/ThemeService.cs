using MusicPlayer.Resources.Styles;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace MusicPlayer.Services
{
    public class ThemeService
    {
        public static ThemeService Instance { get; } = new ThemeService();
        private const string KEY = "AppTheme";
        private string _currentTheme = "DarkTheme";
        public string CurrentTheme => _currentTheme;

        public event EventHandler<string> ThemeChanged;

        private ThemeService() { }

        public void ApplySavedTheme()
        {
            var theme = Preferences.Get(KEY, "DarkTheme");
            ApplyTheme(theme);
        }

        public async void ApplyTheme(string themeName)
        {
            if (string.IsNullOrEmpty(themeName))
                themeName = "DarkTheme";

            if (_currentTheme == themeName)
                return;

            _currentTheme = themeName;

            Debug.WriteLine($"[ThemeService] Applying theme: {themeName}");

            var app = Application.Current;
            if (app?.MainPage == null)
            {
                Debug.WriteLine("[ThemeService] MainPage is null, skipping animation");
                return;
            }

            // 🔹 Fade out
            await app.MainPage.FadeTo(0, 250, Easing.CubicOut);

            // Remove previous themes
            var themeNamespace = typeof(DarkTheme).Namespace;
            var toRemove = app.Resources.MergedDictionaries
                .Where(d => d.GetType().Namespace == themeNamespace)
                .ToList();

            foreach (var d in toRemove)
                app.Resources.MergedDictionaries.Remove(d);

            // Add new theme
            ResourceDictionary dict = themeName switch
            {
                "LightTheme" => new LightTheme(),
                "HalloweenTheme" => new HalloweenTheme(),
                _ => new DarkTheme(),
            };

            app.Resources.MergedDictionaries.Add(dict);
            Preferences.Set(KEY, themeName);

            // 🔹 Fade back in
            await app.MainPage.FadeTo(1, 250, Easing.CubicIn);

            // Notify listeners
            ThemeChanged?.Invoke(this, themeName);
        }
    }
}