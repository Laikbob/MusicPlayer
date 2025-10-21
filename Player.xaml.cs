using CommunityToolkit.Maui.Core.Views; 

namespace MusicPlayer;

public partial class Player : ContentPage
{
    public Player()
    {
        InitializeComponent();
        MusicButton.Clicked += MusicButton_Clicked;
    }

    private void MusicButton_Clicked(object sender, EventArgs e)
    {
        if (MusicPlayer.CurrentState == MediaElementState.Playing)
        {
            MusicPlayer.Pause();
            MusicButton.Text = "Play";
        }
        else
        {
            MusicPlayer.Play();
            MusicButton.Text = "Pause";
        }
    }
}
