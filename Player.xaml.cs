using Microsoft.Maui;
using Plugin.Maui.Audio;
using System.IO;

namespace MusicPlayer;

public partial class Player : ContentPage
{
    IAudioPlayer player;
    bool isPlaying = false;

    List<Track> tracks = new List<Track>();
    int currentTrackIndex = -1;

    public Player()
    {
        InitializeComponent();

        MusicButton.Clicked += MusicButton_Clicked;
        PickFileButton.Clicked += PickFileButton_Clicked;
        NextButton.Clicked += NextButton_Clicked;
        PrevButton.Clicked += PrevButton_Clicked;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 🎃 Анимация тыквы
        await PumpkinImage.ScaleTo(0.5, 0);
        await PumpkinImage.ScaleTo(1.2, 800, Easing.BounceOut);
        await PumpkinImage.ScaleTo(1.0, 400, Easing.CubicOut);

        for (int i = 0; i < 2; i++)
        {
            await PumpkinImage.FadeTo(0.5, 300);
            await PumpkinImage.FadeTo(1.0, 300);
        }

        await Task.Delay(1000);
        AnimationLayout.IsVisible = false;
        PlayerLayout.IsVisible = true;
    }

    private async void PickFileButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var audioFileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp3" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".m4a" } }
            });

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите аудиофайл",
                FileTypes = audioFileTypes
            });

            if (result != null)
            {
                using var stream = await result.OpenReadAsync();
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory);

                tracks.Add(new Track
                {
                    Name = result.FileName,
                    Data = memory.ToArray()
                });

                currentTrackIndex = tracks.Count - 1;
                PlayTrack(currentTrackIndex);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Не удалось выбрать файл: {ex.Message}", "OK");
        }
    }

    private void PlayTrack(int index)
    {
        if (index < 0 || index >= tracks.Count)
            return;

        if (player != null)
        {
            player.Stop();
            player.Dispose();
        }

        // Создаём новый поток из байт
        var stream = new MemoryStream(tracks[index].Data);
        player = AudioManager.Current.CreatePlayer(stream);

        SongTitleLabel.Text = tracks[index].Name;
        MusicButton.Text = "Pause";
        player.Play();
        isPlaying = true;
    }

    private void MusicButton_Clicked(object sender, EventArgs e)
    {
        if (player == null) return;

        if (isPlaying)
        {
            player.Pause();
            MusicButton.Text = "Play";
            isPlaying = false;
        }
        else
        {
            player.Play();
            MusicButton.Text = "Pause";
            isPlaying = true;
        }
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (tracks.Count == 0) return;

        currentTrackIndex = (currentTrackIndex + 1) % tracks.Count;
        PlayTrack(currentTrackIndex);
    }

    private void PrevButton_Clicked(object sender, EventArgs e)
    {
        if (tracks.Count == 0) return;

        currentTrackIndex = (currentTrackIndex - 1 + tracks.Count) % tracks.Count;
        PlayTrack(currentTrackIndex);
    }

    public class Track
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }
}
