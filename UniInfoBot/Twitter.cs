using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CoreTweet;
using CoreTweet.Streaming;

namespace UniInfoBot
{
    public sealed class Twitter : ITwitter
    {
        private readonly string _consumerKey, _consumerSecret, _accessToken, _accessSecret;

        private Tokens _tokens;

        public Twitter()
        {
            using (var fs = new FileStream("TwitterManager.config", FileMode.Open))
            {
                var doc = new XmlDocument();
                doc.Load(fs);

                var element = doc["root"];
                _consumerKey = element["ConsumerKey"].InnerText;
                _consumerSecret = element["ConsumerSecret"].InnerText;
                _accessToken = element["AccessToken"].InnerText;
                _accessSecret = element["AccessSecret"].InnerText;
            }

            _tokens = Tokens.Create(_consumerKey, _consumerSecret, _accessToken, _accessSecret);
        }

        public IEnumerable<Status> GetTweets(params string[] filters)
            => _tokens.Streaming.Filter(track: string.Join(",", filters)).OfType<StatusMessage>().Select(x => x.Status);

        public async Task Tweet(string message, long? inReplyToStatusId = null)
            => await _tokens.Statuses.UpdateAsync(in_reply_to_status_id: inReplyToStatusId, status: message);

        public async Task SendDirectMessage(string screenName, string message)
            => await _tokens.DirectMessages.NewAsync(screenName, message);

        public async Task<string> GetScreenName()
            => (await _tokens.Account.SettingsAsync()).ScreenName;

        public async Task<string> GetName()
            => (await _tokens.Account.UpdateProfileAsync()).Name;

        public async Task SetName(string name)
            => await _tokens.Account.UpdateProfileAsync(name: name);
    }
}
