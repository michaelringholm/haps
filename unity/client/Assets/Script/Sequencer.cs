using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    enum WinType { Xp, Credits, Money };

    class Win
    {
        public static Dictionary<char, Win> WinMap = new Dictionary<char, Win>();
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

            WinMap.Add('A', Create(WinType.Xp, 10, 1));
            WinMap.Add('B', Create(WinType.Xp, 50, 1));
            WinMap.Add('C', Create(WinType.Xp, 100, 1));

            WinMap.Add('D', Create(WinType.Credits, 4, 1));
            WinMap.Add('E', Create(WinType.Credits, 10, 1));
            WinMap.Add('F', Create(WinType.Credits, 20, 1));

            WinMap.Add('G', Create(WinType.Money, 1, 1));

            WinMap.Add('H', Create(WinType.Money, 10, 5));
            WinMap.Add('I', Create(WinType.Money, 100, 10));
            WinMap.Add('J', Create(WinType.Money, 1000, 20));
        }

        public static string GetRequirementText(char c, int level)
        {
            Win w = GetWin(c);
            string result = level < w.LevelRequirement ? string.Format("<color=#ff3010>Kraever level {0}</color>", w.LevelRequirement) : "";
            return result;
        }

        public static string GetWinText(char c)
        {
            // Credits and money are simple
            Win w = GetWin(c);
            if (w.Type == WinType.Credits)
                return string.Format("{0} Kredit", w.Amount);

            if (w.Type == WinType.Xp)
                return string.Format("{0} XP", w.Amount);

            return string.Format("{0} Krone{1}", w.Amount, w.Amount == 1 ? "" : "r");
        }

        public static Win GetWin(char c)
        {
            return WinMap[c];
        }

        static Win Create(WinType type, int amount, int levelRequirement)
        {
            return new Win { Type = type, Amount = amount, LevelRequirement = levelRequirement };
        }

        public WinType Type;
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

        public void Get(bool fullSpin, List<char> row0, List<char> row1, List<char> row2)
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

        private void FillRow(List<char> row, int count)
        {
            row.Clear();
            row.Capacity = count;

            for (int i = 0; i < count; ++i)
            {
                double roll = rnd_.NextDouble();
                char c = GetOne(roll, distribution_);
                row.Add(c);
            }
        }

        private char GetOne(double roll, List<Item> distribution)
        {
            double accumulated = 0.0;
            foreach (var item in distribution)
            {
                accumulated += item.ch;
                if (roll <= accumulated)
                    return item.icon[0];
            }

            Debug.LogErrorFormat("Roll {0} was not matched in distribution", roll);
            return 'A';
        }

        public bool NeedsUpdate()
        {
            return currentSequence_ == null || usageCount >= currentSequence_.suc || forcedWinUsed;
        }

        public void SetSequence(ItemSequence seq)
        {
            currentSequence_ = seq;
            usageCount = 0;
            if (seq.d != null)
                distribution_ = seq.d;

            rnd_ = new System.Random(seq.s);
        }
    }
}
