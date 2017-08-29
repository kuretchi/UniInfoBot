using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CoreTweet;
using CoreTweet.Streaming;

namespace UniInfoBot
{
    public sealed class TwitterManager
    {
        private readonly string _consumerKey, _consumerSecret, _accessToken, _accessSecret;

        private readonly string _screenName, _developerScreenName;

        private Tokens _tokens;

        private IEnumerable<string> _validReplyToStr;

        private static readonly string _underMaintenanceSuffix = "@メンテ中";

        private static readonly IReadOnlyDictionary<string, Difficulty> _difficultySuffixes
            = new Dictionary<string, Difficulty>
        {
            { "easy", Difficulty.Easy },
            { "緑", Difficulty.Easy },
            { "advanced", Difficulty.Advanced },
            { "adv", Difficulty.Advanced },
            { "橙", Difficulty.Advanced },
            { "expert", Difficulty.Expert },
            { "exp", Difficulty.Expert },
            { "ex", Difficulty.Expert },
            { "赤", Difficulty.Expert },
            { "master", Difficulty.Master },
            { "mas", Difficulty.Master },
            { "紫", Difficulty.Master },
        };

        public TwitterManager()
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
                _developerScreenName = element["DeveloperScreenName"].InnerText;
            }

            _tokens = Tokens.Create(_consumerKey, _consumerSecret, _accessToken, _accessSecret);
            _screenName = _tokens.Account.Settings().ScreenName;
            
            // if _screenName == null, throws NullReferenceException
            _validReplyToStr = new[] { $"@{_screenName.ToString()}", $".@{_screenName.ToString()}" };
        }

        public event Action<Status> TweetObserved;

        private void OnTweetObserved(Status status) => TweetObserved?.Invoke(status);

        public async Task StartMonitoringTweet()
        {
            var minSucceededSpan = TimeSpan.FromSeconds(1);
            var firstRetrySpan = TimeSpan.FromSeconds(3);
            var firstRetryCount = 10;
            var secondRetrySpan = TimeSpan.FromMinutes(10);

            while (true)
            {
                for (var i = 0; i < firstRetryCount; )
                {
                    var startedTime = DateTime.Now;

                    try
                    {
                        MonitorTweet();
                    }
                    catch
                    {
                        await Task.WhenAll(
                            this.ChangeStatus(false),
                            this.SendDirectMessageToDeveloper("1st Retry."),
                            Task.Delay(firstRetrySpan));

                        await this.ChangeStatus(true);
                    }

                    i = DateTime.Now - startedTime < minSucceededSpan ? i + 1 : 0;
                }

                await Task.WhenAll(
                    this.ChangeStatus(false),
                    this.SendDirectMessageToDeveloper("2nd Retry."),
                    Task.Delay(secondRetrySpan));

                await this.ChangeStatus(true);
            }
        }

        private void MonitorTweet()
        {
            var statuses = _tokens.Streaming
                .Filter(track: string.Join(",", _validReplyToStr))
                .OfType<StatusMessage>()
                .Select(x => x.Status);

            foreach (var status in statuses)
            {
                OnTweetObserved(status);
            }
        }

        public async Task ChangeStatus(bool isRunning)
        {
            var name = _tokens.Account.UpdateProfileAsync().Result.Name;

            if (isRunning && name.EndsWith(_underMaintenanceSuffix))
            {
                name = name.Replace(_underMaintenanceSuffix, "");
            }

            if (!isRunning && !name.EndsWith(_underMaintenanceSuffix))
            {
                name += _underMaintenanceSuffix;
            }

            await _tokens.Account.UpdateProfileAsync(name: name);
        }

        public async Task SendDirectMessageToDeveloper(string message)
            => await _tokens.DirectMessages.NewAsync(_developerScreenName, message).ConfigureAwait(false);

        private IEnumerable<string> GetWords(string str)
            => str.Split(new[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        public bool NeedsReply(Status status)
            => _validReplyToStr.Contains(GetWords(status.Text).First());

        public (string Name, Difficulty Difficulty) ParseRequest(Status status)
        {
            string name;
            Difficulty difficulty;

            var words = GetWords(status.Text).SkipWhile(x => !_validReplyToStr.Contains(x)).Skip(1);
            
            if (_difficultySuffixes.ContainsKey(words.Last()))
            {
                name = string.Join(" ", words.Take(words.Count() - 1));
                difficulty = _difficultySuffixes[words.Last()];
            }
            else
            {
                name = string.Join(" ", words);
                difficulty = Difficulty.Master;
            }

            return (name, difficulty);
        }

        public async Task Reply(Status status, CalculatedMusic music)
        {
            var sb = new StringBuilder();

            sb.Append("@");
            sb.AppendLine(status.User.ScreenName);
            sb.Append("曲名: ");
            sb.AppendLine(music.Name);
            sb.Append("譜面定数: ");
            sb.AppendFormat("{0:f1}", music.Constant[music.CalculatedDifficulty]);
            sb.AppendLine();
            sb.Append("ノーツ数: ");
            sb.Append(music.Notes[music.CalculatedDifficulty]);
            sb.AppendLine();
            sb.Append("SSS許容: ");

            foreach (var (acceptance, i) in music.AcceptancesForSSS.Select((x, i) => (x, i)))
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("J");
                sb.Append(acceptance.AcceptableJustice);
                sb.Append(" A");
                sb.Append(acceptance.AcceptableAttack);
            }

            sb.AppendLine();
            sb.Append("9900許容: J");
            sb.Append(music.AcceptanceFor9900.AcceptableJustice);

            var text = sb.ToString();
            await _tokens.Statuses.UpdateAsync(in_reply_to_status_id: status.Id, status: text);
        }

        public async Task Reply(Status status, Exception ex)
        {
            var text = $"@{status.User.ScreenName}\n{ex.Message}";
            await _tokens.Statuses.UpdateAsync(in_reply_to_status_id: status.Id, status: text);
        }

        public async Task Reply(Status status, string message)
        {
            var text = $"@{status.User.ScreenName}\n{message}";
            await _tokens.Statuses.UpdateAsync(in_reply_to_status_id: status.Id, status: text);
        }
    }
}
