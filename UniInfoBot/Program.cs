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
            var twitterManager = new TwitterManager();
            await twitterManager.SendDirectMessageToDeveloper("Started.");

            var musicDataManager = new MusicDataManager();
            
            twitterManager.TweetObserved += async (status) =>
            {
                if (!twitterManager.NeedsReply(status))
                {
                    return;
                }

                Music music;
                try
                {
                    music = musicDataManager.GetMusicData(twitterManager.GetMusicName(status));
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

                var result = Calculator.Calculate(music.Level, music.Notes);

                await twitterManager.Reply(status, music.Name, result);
            };

            await Task.WhenAll(
                twitterManager.ChangeStatus(true),
                twitterManager.StartMonitoringTweet());

            await Task.WhenAll(
                twitterManager.ChangeStatus(false),
                twitterManager.SendDirectMessageToDeveloper("Exits."));
        }
    }
}
