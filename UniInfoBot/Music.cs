using System.Collections.Generic;

namespace UniInfoBot
{
    public enum Difficluty
    {
        Easy, Advanced, Expert, Master
    }

    public class Music
    {
        public string Name { get; }

        public IEnumerable<string> Keywords { get; }

        public IReadOnlyDictionary<Difficluty, string> Level { get; }

        public IReadOnlyDictionary<Difficluty, double?> Constant { get; }

        public IReadOnlyDictionary<Difficluty, int?> Notes { get; }

        public Music(dynamic json)
        {
            this.Name = json.Name;
            this.Keywords = json.Keywords;

            this.Level = new Dictionary<Difficluty, string>
            {
                { Difficluty.Easy, json.Level.Easy },
                { Difficluty.Advanced, json.Level.Advanced },
                { Difficluty.Expert, json.Level.Expert },
                { Difficluty.Master, json.Level.Master },
            };

            this.Constant = new Dictionary<Difficluty, double?>
            {
                { Difficluty.Easy, json.Constant.Easy },
                { Difficluty.Advanced, json.Constant.Advanced },
                { Difficluty.Expert, json.Constant.Expert },
                { Difficluty.Master, json.Constant.Master },
            };

            this.Notes = new Dictionary<Difficluty, int?>
            {
                { Difficluty.Easy, json.Notes.Easy },
                { Difficluty.Advanced, json.Notes.Advanced },
                { Difficluty.Expert, json.Notes.Expert },
                { Difficluty.Master, json.Notes.Master },
            };
        }

        public Music(Music music)
        {
            this.Name = music.Name;
            this.Keywords = music.Keywords;
            this.Level = music.Level;
            this.Constant = music.Constant;
            this.Notes = music.Notes;
        }
    }
}
