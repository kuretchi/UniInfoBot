using System.Collections.Generic;
using System.Threading.Tasks;
using CoreTweet;

namespace UniInfoBot
{
    public interface ITwitter
    {
        IEnumerable<Status> GetTweets(params string[] filters);

        Task Tweet(string message, long? inReplyToStatusId = null);

        Task SendDirectMessage(string screenName, string message);

        Task<string> GetScreenName();

        Task<string> GetName();

        Task SetName(string name);
    }
}
