using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2.DataModel;
using HapsApi.DynamoObjects;
using System;
using Amazon;
using HapsApi.StaticData;

namespace HapsApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            //string name = "not set";
            //try
            //{
            //    PrizeFacebook prize = new PrizeFacebook()
            //    {
            //        PrizeId = Guid.NewGuid().ToString(),
            //        Count = 10,
            //        TimestampUtc = DateTime.UtcNow,
            //        Value = 1,
            //        FacebookDonatorId = "fbId",
            //        UserNotificationStyleId = UserNotification.FbName
            //    };

            //    var client = new Amazon.DynamoDBv2.AmazonDynamoDBClient(RegionEndpoint.EUCentral1);
            //    DynamoDBContext ctx = new DynamoDBContext(client, new DynamoDBContextConfig { ConsistentRead = true });
            //    var task = ctx.SaveAsync(prize);
            //    task.Wait();
            //    name = task.Exception == null ? "Success" : "Async error: " + task.Exception.Message;
            //}
            //catch(Exception e)
            //{
            //    name = e.Message;
            //}

            return new string[] { "value1", "yay" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
