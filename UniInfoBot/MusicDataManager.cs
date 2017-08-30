using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Codeplex.Data;

namespace UniInfoBot
{
    public sealed class NotFoundException : Exception
    {
        public override string Message => "曲が見つかりませんでした。";
    }

    public sealed class MusicDataManager : IMusicDataManger
    {
        private HttpClient _client;
        
        private static readonly string _requestUri
            = "https://raw.githubusercontent.com/kuretchi/chunithm-music/master/music.json";

        private static readonly TimeSpan _updateSpan = TimeSpan.FromMinutes(10);

        private List<Music> _musicData;

        private Timer _timer;

        private object _locker;

        public MusicDataManager()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "UniInfoBot (https://twitter.com/uni_info_bot)");
            _musicData = new List<Music>();
            _timer = new Timer(_updateSpan.TotalMilliseconds);
            _timer.Elapsed += async (sender, e) => await UpdateMusicData();
            _locker = new object();

            UpdateMusicData().Wait();

            _timer.Start();
        }

        public Music GetMusicData(string musicKeyword)
        {
            lock (_locker)
            {
                var music = _musicData.FirstOrDefault(x
                    => x.Name.IndexOf(musicKeyword, StringComparison.OrdinalIgnoreCase) >= 0);

                if (music == default(Music))
                {
                    throw new NotFoundException();
                }

                return music;
            }
        }

        private async Task UpdateMusicData()
        {
            var json = DynamicJson.Parse(await _client.GetStringAsync(_requestUri));

            lock (_locker)
            {
                _musicData.Clear();

                for (var i = 0; json.IsDefined(i); i++)
                {
                    _musicData.Add(new Music(json[i]));
                }
            }
        }
    }
}
