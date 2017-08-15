using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Codeplex.Data;

namespace UniInfoBot
{
    public sealed class Music
    {
        public int MusicId { get; set; }

        public string Name { get; set; }

        public double Level { get; set; }

        public int Notes { get; set; }
    }

    public sealed class MusicDataContext : DbContext
    {
        public DbSet<Music> Musics { get; set; }

        public MusicDataContext() : base(nameof(MusicDataContext)) { }
    }

    public sealed class NotFoundException : Exception
    {
        public override string Message => "曲が見つかりませんでした。";
    }

    public sealed class MusicDataManager
    {
        private HttpClient _client;
        
        private static readonly string _requestUri = "http://chuniviewer.net/GetMusicData.php";

        private static readonly TimeSpan _updateSpan = TimeSpan.FromMinutes(10);

        private Timer _timer;

        private object _locker;

        public MusicDataManager()
        {
            _client = new HttpClient();
            _timer = new Timer(_updateSpan.TotalMilliseconds);
            _timer.Elapsed += async (sender, e) => await UpdateDatabase();
            _locker = new object();

            ResetDatabase();
            UpdateDatabase().Wait();

            _timer.Start();
        }

        private async Task<dynamic> DownloadJsonData()
        {
            var response = await _client.PostAsync(_requestUri, null);
            var json = await response.Content.ReadAsStringAsync();

            const int maxLength = 10000;

            for (var i = 0; i < maxLength; i++)
            {
                json = json.Replace($"\"{i}\"", $"\"_{i}\"");
            }

            return DynamicJson.Parse(json);
        }

        private async Task UpdateDatabase()
        {
            var json = await DownloadJsonData();

            lock (_locker)
            {
                using (var context = new MusicDataContext())
                {
                    const int maxLength = 10000;

                    for (var i = 0; i < maxLength; i++)
                    {
                        if (!json.IsDefined($"_{i}"))
                        {
                            continue;
                        }

                        Music data;
                        var name = (string)json[$"_{i}"]["name"];

                        if (json[$"_{i}"]["difficulty"].IsDefined($"_{3}"))
                        {
                            var level = (double)json[$"_{i}"]["difficulty"][$"_{3}"]["pattern_constant"];
                            var notes = (int)Convert.ToInt32(json[$"_{i}"]["difficulty"][$"_{3}"]["notes"]);
                            data = new Music() { Name = name, Level = level, Notes = notes };
                        }
                        else if (json[$"_{i}"]["difficulty"].IsDefined(3))
                        {
                            var level = (double)json[$"_{i}"]["difficulty"][3]["pattern_constant"];
                            var notes = (int)Convert.ToInt32(json[$"_{i}"]["difficulty"][3]["notes"]);
                            data = new Music() { Name = name, Level = level, Notes = notes };
                        }
                        else
                        {
                            continue;
                        }

                        var music = context.Musics.SingleOrDefault(x => x.Name == data.Name);

                        if (music == default(Music))
                        {
                            context.Musics.Add(data);
                        }
                        else
                        {
                            music = data;
                        }

                        context.SaveChanges();
                    }
                }
            }
        }

        private void ResetDatabase()
        {
            lock (_locker)
            {
                using (var context = new MusicDataContext())
                {
                    context.Musics.RemoveRange(context.Musics);
                    context.SaveChanges();
                }
            }
        }

        public Music GetMusicData(string musicKeyword)
        {
            lock (_locker)
            {
                using (var context = new MusicDataContext())
                {
                    var music = context.Musics.ToArray().FirstOrDefault(x
                        => x.Name.IndexOf(musicKeyword, StringComparison.OrdinalIgnoreCase) >= 0);

                    if (music == default(Music))
                    {
                        throw new NotFoundException();
                    }

                    return music;
                }
            }
        }
    }
}
