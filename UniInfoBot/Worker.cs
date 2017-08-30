using System;
using System.Threading.Tasks;
using CoreTweet;
using SimpleInjector;

namespace UniInfoBot
{
    public class Worker
    {
        private IMusicDataManger _musicDataManager;

        private ITwitterManager _twitterManager;

        public Worker(Container container)
        {
            _musicDataManager = container.GetInstance<IMusicDataManger>();
            _twitterManager = container.GetInstance<ITwitterManager>();

            _twitterManager.TweetObserved += this.Work;
        }

        public async Task Start()
        {
            await Task.WhenAll(
                _twitterManager.ChangeStatus(true),
                _twitterManager.SendDirectMessageToDeveloper("Started."),
                _twitterManager.StartMonitoringTweet());

            await Task.WhenAll(
                _twitterManager.ChangeStatus(false),
                _twitterManager.SendDirectMessageToDeveloper("Exits."));
        }

        private async void Work(Status status)
        {
            if (!_twitterManager.NeedsReply(status))
            {
                return;
            }

            var (name, difficulty) = _twitterManager.ParseRequest(status);
            Music music;
            try
            {
                music = _musicDataManager.GetMusicData(name);
            }
            catch (NotFoundException ex)
            {
                await _twitterManager.Reply(status, ex);
                return;
            }
            catch (Exception ex)
            {
                await Task.WhenAll(
                    _twitterManager.Reply(status, "エラーが発生しました。"),
                    _twitterManager.SendDirectMessageToDeveloper(
                        $"Exception Thrown. \nTweet ID: {status.Id}\n{ex}"));
                return;
            }

            var calculatedMusic = Calculator.Calculate(music, difficulty);
            await _twitterManager.Reply(status, calculatedMusic);
        }
    }
}
