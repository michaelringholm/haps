using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrubloServer
{
    class Simulator
    {
        private static char GetOne(double roll, List<Item> distribution)
        {
            double accumulated = 0.0;
            foreach (var item in distribution)
            {
                accumulated += item.ch;
                if (roll <= accumulated)
                    return item.icon[0];
            }

            Console.WriteLine("Roll {0} was not matched in distribution", roll);
            return 'A';
        }

        public static void Run()
        {
            // 15 combinations = 100% win (winline = 1, + nudges)
            // 1 - ((1  - (ch * ch * ch)) ^ 15)
            Dictionary<char, double> match = new Dictionary<char, double>();
            Random rnd = new Random();
            var dist = Distribution.CurrentDistribution.OrderBy(i => i.ch).ToList();

            foreach (var item in dist)
                match[item.icon[0]] = 0;

            int count = 100000;
            for (int i = 0; i < count; ++i)
            {
                foreach (var item in dist)
                {
                    bool won = false;
                    for (int j = 0; j < 15; ++j)
                    {
                        char c = item.icon[0];
                        double chance = item.ch;
                        double negChance = 1.0 - chance;
                        double chance3 = chance * chance * chance;
                        double chanceNone = negChance * negChance * negChance;
                        double chanceExactlyOne = 3 * chance * negChance * negChance;
                        double chance2 = 1.0 - chanceNone - chanceExactlyOne;
                        double roll = rnd.NextDouble();
                        if (roll < chance3)
                        {
                            // Direct
                            match[c] = match[c] + 1;
                            won = true;
                            break;
                        }
                        else if (roll < chance2)
                        {
                            // Hold 2, then win
                            match[c] = match[c] + 1;
                            won = true;
                            break;
                        }
                    }
                    if (won)
                        break;
                }
            }

            var pairs = match.OrderBy(i => i.Value).ToList();
            double recip = 1.0 / pairs.Sum(p => p.Value);
            foreach (var pair in pairs)
            {
                Console.WriteLine("{0} = {1,8:0.000000000}%", pair.Key, (pair.Value / count) * 100);
            }
        }
    }
}
