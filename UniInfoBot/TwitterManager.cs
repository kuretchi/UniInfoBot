﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CoreTweet;
using CoreTweet.Streaming;

namespace UniInfoBot
{
    public delegate void TweetEventHandler(Status status);

    public sealed class TwitterManager
    {
        private readonly string _consumerKey, _consumerSecret, _accessToken, _accessSecret;

        private readonly string _screenName, _developerScreenName;

        private Tokens _tokens;

        private IEnumerable<string> _validReplyToStr;

        private static readonly string _underMaintenanceSuffix = "@メンテ中";

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

        public event TweetEventHandler TweetObserved;

        private void OnTweetObserved(Status status) => TweetObserved?.Invoke(status);

        public async Task<Status> StartMonitoringTweet()
        {
            var observable = _tokens.Streaming
                .FilterAsObservable(track: string.Join(",", _validReplyToStr))
                .OfType<StatusMessage>()
                .Select(x => x.Status);

            var firstRetrySpan = TimeSpan.FromSeconds(3);
            var firstRetryCount = 10;
            var secondRetrySpan = TimeSpan.FromMinutes(10);

            observable
                .Catch(observable)
                .Catch(observable.DelaySubscription(firstRetrySpan))
                .Retry(firstRetryCount)
                .Catch(observable.DelaySubscription(secondRetrySpan))
                .Repeat()
                .Subscribe((Status status) => OnTweetObserved(status));

            return await observable;
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

        private IEnumerable<string> GetWords(Status status)
            => status.Text.Split(new[] { " ", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        public bool NeedsReply(Status status)
            => _validReplyToStr.Contains(GetWords(status).First());

        public string GetMusicName(Status status)
            => GetWords(status).SkipWhile(x => !_validReplyToStr.Contains(x)).Skip(1).Aggregate((a, s) => a + " " + s);

        public async Task Reply(Status status, string musicName, CalculationResult result)
        {
            var sb = new StringBuilder();

            sb.Append("@");
            sb.AppendLine(status.User.ScreenName);
            sb.Append("曲名: ");
            sb.AppendLine(musicName);
            sb.Append("譜面定数: ");
            sb.AppendFormat("{0:f1}", result.Level);
            sb.AppendLine();
            sb.Append("ノーツ数: ");
            sb.Append(result.Notes);
            sb.AppendLine();
            sb.Append("SSS許容: ");

            foreach (var (acceptance, i) in result.AcceptancesForSSS.Select((x, i) => (x, i)))
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
            sb.Append(result.AcceptanceFor9900.AcceptableJustice);

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
