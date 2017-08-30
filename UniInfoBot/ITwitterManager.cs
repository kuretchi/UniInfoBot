using System;
using System.Threading.Tasks;
using CoreTweet;

namespace UniInfoBot
{
    interface ITwitterManager
    {
        event Action<Status> TweetObserved;

        Task StartMonitoringTweet();

        Task ChangeStatus(bool isRunning);

        Task SendDirectMessageToDeveloper(string message);

        bool NeedsReply(Status status);

        (string Name, Difficulty Difficulty) ParseRequest(Status status);

        Task Reply(Status status, CalculatedMusic music);

        Task Reply(Status status, Exception ex);

        Task Reply(Status status, string message);
    }
}
