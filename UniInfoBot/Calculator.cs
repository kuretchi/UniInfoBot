using System.Collections.Generic;
using System.Linq;

namespace UniInfoBot
{
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

    public sealed class CalculationResult
    {
        public double Level { get; }

        public int Notes { get; }

        public IEnumerable<AcceptanceForSSS> AcceptancesForSSS { get; }

        public AcceptanceFor9900 AcceptanceFor9900 { get; }

        public CalculationResult(double level, int notes, IEnumerable<AcceptanceForSSS> acceptancesForSSS, AcceptanceFor9900 acceptanceFor9900)
        {
            this.Level = level;
            this.Notes = notes;
            this.AcceptancesForSSS = acceptancesForSSS;
            this.AcceptanceFor9900 = acceptanceFor9900;
        }
    }

    public static class Calculator
    {
        public static CalculationResult Calculate(double level, int notes)
        {
            const double justiceRate = 0.04;

            const double scoreMax = 1010000;
            const double score9900 = 1009900;
            const double scoreSSS = 1007500;

            var scorePerJusticeCritical = scoreMax / notes;
            var scorePerJustice = scoreMax / notes * (100.0 / 101.0);
            var scorePerAttack = scoreMax / notes * (50.0 / 101.0);

            var justiceCountForSSS = new int?[] { null, (int)(notes * justiceRate), null };
            var attackCountForSSS = new[] { 0, 0, 0 };

            for (var i = 0; ; i++)
            {
                var score = (int)(scorePerJusticeCritical * (notes - justiceCountForSSS[1] - i)
                    + scorePerJustice * justiceCountForSSS[1]
                    + scorePerAttack * i);

                if (score >= scoreSSS)
                {
                    attackCountForSSS[1] = i;
                }
                else
                {
                    break;
                }
            }

            attackCountForSSS[0] = attackCountForSSS[1] + 1;
            attackCountForSSS[2] = attackCountForSSS[1] - 1;

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; ; j++)
                {
                    var score = (int)(scorePerJusticeCritical * (notes - attackCountForSSS[i] - j)
                        + scorePerJustice * j
                        + scorePerAttack * attackCountForSSS[i]);

                    if (score >= scoreSSS)
                    {
                        justiceCountForSSS[i] = j;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var justiceCountFor9900 = 0;

            for (var i = 0; ; i++)
            {
                var score = (int)(scorePerJusticeCritical * (notes - i) + scorePerJustice * i);

                if (score >= score9900)
                {
                    justiceCountFor9900 = i;
                }
                else
                {
                    break;
                }
            }

            return new CalculationResult(level, notes,
                justiceCountForSSS
                    .Zip(attackCountForSSS, (x, y) => (x, y))
                    .Where(x => x.Item1.HasValue)
                    .Select(x => new AcceptanceForSSS(x.Item1.Value, x.Item2)),
                new AcceptanceFor9900(justiceCountFor9900));
        }
    }
}
