using Microsoft.AspNetCore.Mvc;

namespace HapsApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        class Info
        {
            public string deviceId { get; set; }
            public string fbId { get; set; }
            public string fbAccessToken { get; set; }
        }

        [HttpGet]
        public string Login(string deviceId, string fbId, string fbAccessToken)
        {
            string s = "";
            s += deviceId + ", ";
            s += fbId + ", ";
            s += fbAccessToken;
            return s;
        }
    }
}
