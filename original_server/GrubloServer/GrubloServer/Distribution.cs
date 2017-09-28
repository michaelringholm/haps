using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrubloServer
{
    class Distribution
    {
        public static List<Item> CurrentDistribution = new List<Item>();

        public static void CreateDistribution()
        {
            // Money wins are treated separately. Since money is a fixed resource 
            // and number of players is unknown we will hand out money when appropriate.
            // Money icons are still present in the distribution or they would never
            // show up for the user, except when she wins. But app will filter out any wins
            // and only win when explicitly told to.
            // When told to win, app should randomly present...
            //  a) Instant win
            //  2) Nudge win (requires just correct nudge)
            //  3) Hold win (present two icons, and if player hold both they will win on next spin)

            CurrentDistribution.Add(Item.Create('A', 1.2)); // 10 xp
            CurrentDistribution.Add(Item.Create('B', 0.7)); // 50 xp
            CurrentDistribution.Add(Item.Create('C', 0.5)); // 100 xp

            CurrentDistribution.Add(Item.Create('D', 1.2)); // 5 credits
            CurrentDistribution.Add(Item.Create('E', 0.7)); // 10 credits
            CurrentDistribution.Add(Item.Create('F', 0.5)); // 20 credits

            CurrentDistribution.Add(Item.Create('G', 0.1)); // 1 kr

            CurrentDistribution.Add(Item.Create('H', 0.05)); // 10 kr
            CurrentDistribution.Add(Item.Create('I', 0.02)); // 100 kr
            CurrentDistribution.Add(Item.Create('J', 0.01)); // 1000 kr

            double sum = CurrentDistribution.Sum(i => i.ch);
            CurrentDistribution.ForEach(i => i.ch *= 1.0 / sum);
            double newSum = CurrentDistribution.Sum(i => i.ch);
        }
    }
}
