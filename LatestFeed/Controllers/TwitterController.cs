using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LatestFeed.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LatestFeed.Controllers
{
    public class TwitterController : Controller
    {
        [HttpPost]
        public IActionResult Index(string ScreenName, int NumberTweets)
        {
           
                IEnumerable<string> twitts = this.GetTweets(userName: ScreenName, count: NumberTweets).Result;
                int numTweets = 0;
                foreach (var t in twitts)
                {
                    // Console.WriteLine(t + "\n");
                    ViewData[string.Format("Message{0}", numTweets++)] = t + "\n";
                }

                //ViewData["Message"] = "Hello " + name;
                ViewData["NumTimes"] = NumberTweets;

           

            return View();
            
            //return View();
        }


        public async Task<string> GetAccessToken()
        {
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth2/token ");
            var customerInfo = Convert.ToBase64String(new UTF8Encoding()
                                      .GetBytes("tdHnm9ytsQ0Ry0QSCXykOh03G" + ":" + "6gLDfqSol66u4FuPSAfHxRmEtVZJ9OUO4vHGgy9HEglriLwQ8i"));
            request.Headers.Add("Authorization", "Basic " + customerInfo);
            request.Content = new StringContent("grant_type=client_credentials",
                                                    Encoding.UTF8, "application/x-www-form-urlencoded");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            string json = await response.Content.ReadAsStringAsync();
            // var serializer = new JavaScriptSerializer();
            dynamic item = JsonConvert.DeserializeObject<object>(json);
            return item["access_token"];
        }



        public async Task<IEnumerable<string>> GetTweets(string userName, int count, string accessToken = null)
        {
            if (accessToken == null)
            {
                accessToken = await GetAccessToken();
            }

            var requestUserTimeline = new HttpRequestMessage(HttpMethod.Get,
                string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={1}&count={0}&trim_user=1&exclude_replies=1", count, userName));

            requestUserTimeline.Headers.Add("Authorization", "Bearer " + accessToken);
            var httpClient = new HttpClient();
            HttpResponseMessage responseUserTimeLine = await httpClient.SendAsync(requestUserTimeline);
            //   var serializer = new JavaScriptSerializer();
            dynamic json = JsonConvert.DeserializeObject<object>(await responseUserTimeLine.Content.ReadAsStringAsync());
            var enumerableTweets = (json as IEnumerable<dynamic>);

            if (enumerableTweets == null)
            {
                return null;
            }
            return enumerableTweets.Select(t => (string)(t["text"].ToString()));
        }
    }
}
