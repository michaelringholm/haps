using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Commands
{
    [System.Serializable]
    public class CreateUser
    {
        public int __id0__ = 0;
        public string info = string.Empty;
    }

    // Sequence --------->
    [System.Serializable]
    public class RequestSequence
    {
        public int __id1__ = 0;
        public string uid = "";
        public int distId = 0;
    }

    [System.Serializable]
    public class Item
    {
        public static Item Create(char icon, double chance)
        {
            return new Item() { icon = icon.ToString(), ch = chance };
        }
        public string icon = "";
        public double ch = 0.0;
    }

    [System.Serializable]
    public class ItemSequence
    {
        public char[] fw = new char[0]; // forced win, use instead of seed
        public int s = 0;   // seed
        public int suc = 10; // seed usage count
        public uint f = 0;   // flags
        public int did = 0;  // distribution id
        public List<Item> d = new List<Item>(); // distribution
    }
    // <--------- Sequence


    // Win --------->
    [System.Serializable]
    public class ClaimWin
    {
        public int __id2__ = 0;
        public string uid = "";
        public string seq = "";
        public string donId = "";
    }

    [System.Serializable]
    public class ConfirmedWin
    {
        public int amount = 0;
        public char Class = ' '; // Donated by xxx, or...
        public string info = "";
    }
    // <--------- Win
}
