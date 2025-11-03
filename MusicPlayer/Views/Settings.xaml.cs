using MusicPlayer.Services;

namespace MusicPlayer.Views;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();
        UpdateChecks();
    }

    private void UpdateChecks()
    {
        var current = ThemeService.Instance.CurrentTheme;

        // Подсветка выбранной темы
        BtnDarkTheme.BackgroundColor = current == "DarkTheme" ? Colors.LightGray : Colors.Transparent;
        BtnLightTheme.BackgroundColor = current == "LightTheme" ? Colors.LightGray : Colors.Transparent;
        BtnHalloweenTheme.BackgroundColor = current == "HalloweenTheme" ? Colors.LightGray : Colors.Transparent;
    }

    private void OnLanguageClicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is string lang)
        {
            LocalizationResourceManager.Instance.SetCulture(new System.Globalization.CultureInfo(lang));
        }
    }

    private void OnDarkClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("DarkTheme");
        UpdateChecks();
    }

    private void OnLightClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("LightTheme");
        UpdateChecks();
    }

    private void OnHalloweenClicked(object sender, EventArgs e)
    {
        ThemeService.Instance.ApplyTheme("HalloweenTheme");
        UpdateChecks();
    }
}
