using Amazon;
using Amazon.DynamoDBv2.DataModel;
using HapsApi.DynamoObjects;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HapsApi.BL
{
    public class RowCache<T>
    {
        private TimeSpan refreshInterval_;
        private int maxRows_;
        private Timer refreshTimer_;

        public RowCache(int maxRows, TimeSpan refreshInterval)
        {
            Logger.LogLine("Creating RowCache");

            maxRows_ = maxRows;
            refreshInterval_ = refreshInterval;
            refreshTimer_ = new Timer(this.OnRefreshTimerTick, null, 0, Timeout.Infinite);
        }

        void OnRefreshTimerTick(object state)
        {
            RefreshCache();
            refreshTimer_.Change((int)refreshInterval_.TotalMilliseconds, 0);
        }

        void RefreshCache()
        {
            var client = new Amazon.DynamoDBv2.AmazonDynamoDBClient(RegionEndpoint.EUCentral1);
            DynamoDBContext ctx = new DynamoDBContext(client);
            var search = ctx.ScanAsync<T>(new List<ScanCondition>());
            var task = search.GetRemainingAsync();
            task.Wait();

            var list = task.Result;
            Logger.LogLine("List len: " + list.Count);
        }
    }
}
