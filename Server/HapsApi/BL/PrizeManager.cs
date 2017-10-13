using HapsApi.DynamoObjects;
using System;

namespace HapsApi.BL
{
    public class PrizeManager
    {
        private static PrizeManager instance_;
        public static PrizeManager Instance { get; private set; }

        RowCache<PrizeFacebook> prizeFacebookCache_;
        public bool HasPrizes => true;

        internal static void Init()
        {
            if (instance_ != null)
                return;

            instance_ = new PrizeManager();
        }

        public PrizeManager()
        {
            prizeFacebookCache_ = new RowCache<PrizeFacebook>(10000, TimeSpan.FromMinutes(30));
        }
    }
}
