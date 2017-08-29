using System;
using System.Threading.Tasks;

namespace UniInfoBot
{
    public static class Program
    {
        public static void Main()
        {
            Execute().GetAwaiter().GetResult();
        }

        public static async Task Execute()
        {
            var musicDataManager = new MusicDataManager();
            var twitterManager = new TwitterManager();

            twitterManager.TweetObserved += async (status) =>
            {
                if (!twitterManager.NeedsReply(status))
                {
                    return;
                }

                var (name, difficulty) = twitterManager.ParseRequest(status);
                Music music;
                try
                {
                    music = musicDataManager.GetMusicData(name);
                }
                catch (NotFoundException ex)
                {
                    await twitterManager.Reply(status, ex);
                    return;
                }
                catch (Exception ex)
                {
                    await Task.WhenAll(
                        twitterManager.Reply(status, "エラーが発生しました。"),
                        twitterManager.SendDirectMessageToDeveloper(
                            $"Exception Thrown. \nTweet ID: {status.Id}\n{ex}"));
                    return;
                }

                var calculatedMusic = Calculator.Calculate(music, difficulty);
                await twitterManager.Reply(status, calculatedMusic);
            };

            await Task.WhenAll(
                twitterManager.ChangeStatus(true),
                twitterManager.SendDirectMessageToDeveloper("Started."),
                twitterManager.StartMonitoringTweet());

            await Task.WhenAll(
                twitterManager.ChangeStatus(false),
                twitterManager.SendDirectMessageToDeveloper("Exits."));
        }
    }
}
