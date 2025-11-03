using MusicPlayer.Data; 

namespace MusicPlayer
{
    public partial class App : Application
    {
        private static MusicDatabase _database;

        // ✅ Глобальное свойство для доступа к базе данных
        public static MusicDatabase Database
        {
            get
            {
                if (_database == null)
                {
                    var dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "music.db3");
                    _database = new MusicDatabase(dbPath);
                }
                return _database;
            }
        }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // ✅ Используем твой AppShell
            return new Window(new AppShell());
        }
    }
}