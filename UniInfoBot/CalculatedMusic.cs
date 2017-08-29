using System.Collections.Generic;

namespace UniInfoBot
{
    public class CalculatedMusic : Music
    {
        public Difficulty CalculatedDifficulty { get; }

        public IEnumerable<AcceptanceForSSS> AcceptancesForSSS { get; }

        public AcceptanceFor9900 AcceptanceFor9900 { get; }

        public CalculatedMusic(Music music, Difficulty calculatedDifficulty,
            IEnumerable<AcceptanceForSSS> acceptancesForSSS, AcceptanceFor9900 acceptanceFor9900)
            : base(music)
        {
            this.CalculatedDifficulty = calculatedDifficulty;
            this.AcceptancesForSSS = acceptancesForSSS;
            this.AcceptanceFor9900 = acceptanceFor9900;
        }
    }

    public sealed class AcceptanceForSSS
    {
        public int AcceptableJustice { get; }

        public int AcceptableAttack { get; }

        public AcceptanceForSSS(int acceptableJustice, int acceptableAttack)
        {
            this.AcceptableJustice = acceptableJustice;
            this.AcceptableAttack = acceptableAttack;
        }
    }

    public sealed class AcceptanceFor9900
    {
        public int AcceptableJustice { get; }

        public AcceptanceFor9900(int acceptableJustice)
        {
            this.AcceptableJustice = acceptableJustice;
        }
    }
}
