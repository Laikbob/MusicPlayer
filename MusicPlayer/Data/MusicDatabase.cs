using SQLite;
using MusicPlayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicPlayer.Data
{
    public class MusicDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public MusicDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Track>().Wait();
        }

        public Task<List<Track>> GetTracksAsync()
        {
            return _database.Table<Track>().ToListAsync();
        }

        public Task<int> SaveTrackAsync(Track track)
        {
            if (track.Id != 0)
                return _database.UpdateAsync(track);
            else
                return _database.InsertAsync(track);
        }

        public Task<int> DeleteTrackAsync(Track track)
        {
            return _database.DeleteAsync(track);
        }
    }
}
