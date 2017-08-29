using System.Collections.Generic;
using System.Linq;

namespace UniInfoBot
{
    public static class Calculator
    {
        public static CalculatedMusic Calculate(Music music, Difficulty difficulty)
        {
            if (!music.Notes[difficulty].HasValue)
            {
                return null;
            }

            var notes = music.Notes[difficulty].Value;

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

            return new CalculatedMusic(music, difficulty,
                justiceCountForSSS
                    .Zip(attackCountForSSS, (x, y) => (AcceptableJustice: x, AcceptableAttack: y))
                    .Where(x => x.AcceptableJustice.HasValue)
                    .Select(x => new AcceptanceForSSS(x.AcceptableJustice.Value, x.AcceptableAttack)),
                new AcceptanceFor9900(justiceCountFor9900));
        }
    }
}
