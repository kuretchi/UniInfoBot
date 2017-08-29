using System.Collections.Generic;

namespace UniInfoBot
{
    public enum Difficulty
    {
        Easy, Advanced, Expert, Master
    }

    public class Music
    {
        public string Name { get; }

        public IEnumerable<string> Keywords { get; }

        public IReadOnlyDictionary<Difficulty, string> Level { get; }

        public IReadOnlyDictionary<Difficulty, double?> Constant { get; }

        public IReadOnlyDictionary<Difficulty, int?> Notes { get; }

        public Music(dynamic json)
        {
            this.Name = json.Name;
            this.Keywords = json.Keywords;

            this.Level = new Dictionary<Difficulty, string>
            {
                { Difficulty.Easy, json.Level.Easy },
                { Difficulty.Advanced, json.Level.Advanced },
                { Difficulty.Expert, json.Level.Expert },
                { Difficulty.Master, json.Level.Master },
            };

            this.Constant = new Dictionary<Difficulty, double?>
            {
                { Difficulty.Easy, json.Constant.Easy },
                { Difficulty.Advanced, json.Constant.Advanced },
                { Difficulty.Expert, json.Constant.Expert },
                { Difficulty.Master, json.Constant.Master },
            };

            this.Notes = new Dictionary<Difficulty, int?>
            {
                { Difficulty.Easy, json.Notes.Easy },
                { Difficulty.Advanced, json.Notes.Advanced },
                { Difficulty.Expert, json.Notes.Expert },
                { Difficulty.Master, json.Notes.Master },
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
