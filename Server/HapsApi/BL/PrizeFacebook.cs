using Amazon.DynamoDBv2.DataModel;
using System;

namespace HapsApi.DynamoObjects
{
    [DynamoDBTable("PrizesFacebook")]
    public class PrizeFacebook
    {
        [DynamoDBHashKey] public string PrizeId { get; set; }
        [DynamoDBRangeKey] public DateTime TimestampUtc { get; set; }
        public string FacebookDonatorId { get; set; }
        public int Count { get; set; }
        [DynamoDBIgnore] public int LockedCount { get; set; }
        public double Value { get; set; }
        public string UserNotificationStyleId { get; set; }
        [DynamoDBVersion] public int? VersionNumber { get; set; }
    }
}
