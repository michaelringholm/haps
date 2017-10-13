using System;
using System.Collections.Generic;

namespace TestDataCsv
{
    class Program
    {
        public class PrizeFacebook
        {
            public string PrizeId { get; set; }
            public DateTime TimestampUtc { get; set; }
            public string FacebookDonatorId { get; set; }
            public int Count { get; set; }
            public double Value { get; set; }
            public string UserNotificationStyleId { get; set; }
        }

        static List<string> lines = new List<string>();

        static void Add(params string[] args)
        {
            lines.Add(string.Join(',', args) + Environment.NewLine);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Add("PrizeId", "TimestampUtc", "FacebookDonatorId", "Count", "Value", "UserNotificationStyleId");
        }
    }
}
