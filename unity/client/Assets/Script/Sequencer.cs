using HapsCommands;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    enum WinType { None, Xp, Circles, Pet, Money };

    class Win
    {
        public static Dictionary<int, Win> WinMap = new Dictionary<int, Win>();

        static Win()
        {
            //CurrentDistribution.Add(Item.Create('A', 0.1)); // 10 xp
            //CurrentDistribution.Add(Item.Create('B', 0.1)); // 50 xp
            //CurrentDistribution.Add(Item.Create('C', 0.1)); // 100 xp

            //CurrentDistribution.Add(Item.Create('D', 0.1)); // 5 credits
            //CurrentDistribution.Add(Item.Create('E', 0.1)); // 10 credits
            //CurrentDistribution.Add(Item.Create('F', 0.1)); // 20 credits

            //CurrentDistribution.Add(Item.Create('G', 0.1)); // 1 kr

            //CurrentDistribution.Add(Item.Create('H', 0.1)); // 10 kr
            //CurrentDistribution.Add(Item.Create('I', 0.1)); // 100 kr
            //CurrentDistribution.Add(Item.Create('J', 0.1)); // 1000 kr

            WinMap.Add(1, Create(WinType.Xp, 10, 1));
            WinMap.Add(2, Create(WinType.Xp, 50, 1));
            WinMap.Add(3, Create(WinType.Xp, 100, 1));
        }

        public static string GetRequirementText(int c, int level)
        {
            Win w = GetWin(c);
            string result = level < w.LevelRequirement ? string.Format("<color=#ff3010>Kraever level {0}</color>", w.LevelRequirement) : "";
            return result;
        }

        public static string GetWinText(int c)
        {
            // Credits and money are simple
            Win w = GetWin(c);

            if (w.Type == WinType.Xp)
                return string.Format("{0} XP", w.Amount);

            return string.Format("{0} Krone{1}", w.Amount, w.Amount == 1 ? "" : "r");
        }

        public static Win GetWin(int c)
        {
            Win result;
            if (!WinMap.TryGetValue(c, out result))
            {
                result = new Win();
                AppLog.LogLine("No win defined for item: {0}", c);
            }

            return result;
        }

        static Win Create(WinType type, int amount, int levelRequirement)
        {
            return new Win { Type = type, Amount = amount, LevelRequirement = levelRequirement };
        }

        public WinType Type = WinType.None;
        public int Amount;
        public int LevelRequirement;
    }

    class Sequencer
    {
        public class Step
        {
            // A cheater can easily play perfectly, so don't store winline or hold flags.
            // Store full/1-down and always reroll all three rows fully so server can still verify.
            public bool fullSpin; // vs. 1-down
        }

        private const int SpinCount0 = 10;
        private const int SpinCount1 = 15;
        private const int SpinCount2 = 20;

        private List<Step> executedSteps_ = new List<Step>();
        private System.Random rnd_;

        private List<Item> distribution_ = new List<Item>();
        private ItemSequence currentSequence_;
        private int usageCount;
        private bool forcedWinUsed = false;

        public void Get(bool fullSpin, List<int> row0, List<int> row1, List<int> row2)
        {
            executedSteps_.Add(new Step() { fullSpin = fullSpin });
            FillRow(row0, SpinCount0);
            FillRow(row1, SpinCount1);
            FillRow(row2, SpinCount2);

            //char c = 'G';
            //row0[0] = c;
            //row1[1] = c;
            //row2[2] = c;
        }

        private void FillRow(List<int> row, int count)
        {
            row.Clear();
            row.Capacity = count;

            for (int i = 0; i < count; ++i)
            {
                double roll = rnd_.NextDouble();
                int c = GetOne(roll, distribution_);
                row.Add(c);
            }
        }

        private int GetOne(double roll, List<Item> distribution)
        {
            double accumulated = 0.0;
            foreach (var item in distribution)
            {
                accumulated += item.ch;
                if (roll <= accumulated)
                    return item.code;
            }

            Debug.LogErrorFormat("Roll {0} was not matched in distribution", roll);
            return 0;
        }

        public bool NeedsUpdate()
        {
            return currentSequence_ == null || usageCount >= currentSequence_.suc || forcedWinUsed;
        }

        public void SetSequence(ItemSequence seq)
        {
            currentSequence_ = seq;
            double sum = 0.0;

            for (int i = 0; i < seq.d.Count; ++i)
                sum += seq.d[i].ch;

            double scale = 1.0 / sum;

            for (int i = 0; i < seq.d.Count; ++i)
                seq.d[i].ch *= scale;

            double newSum = 0.0;
            for (int i = 0; i < seq.d.Count; ++i)
                newSum += seq.d[i].ch;

            AppLog.LogLine("Normalized sum: {0}", newSum);

            //double sum = seq.Sum(i => i.ch);
            //seq.ForEach(i => i.ch *= 1.0 / sum);
            //double newSum = seq.Sum(i => i.ch);
            usageCount = 0;
            if (seq.d != null)
                distribution_ = seq.d;

            rnd_ = new System.Random(seq.s);
        }
    }
}
