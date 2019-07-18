using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
namespace LatestFeed.Controllers
{
    public class TwitterController : Controller
    {
        //This function will get x tweets from a user using his/her screen name.
        [HttpPost]
        public IActionResult Index(string ScreenName, int NumberTweets)
        {
            //Call the GetTweets method
            IEnumerable<string> twitts = this.GetTweets(ScreenName, count:NumberTweets).Result;
            
            int numTweets = 0;
            //Set the ScreenName of the twitter user inorder to access it in View. 
            ViewData["Name"] = ScreenName;

            try
            {
                //Get tweets and attach them to Viewdata by creating a set of messages
                foreach (var t in twitts)
                {
                    ViewData[string.Format("Message{0}", numTweets++)] = t + "\n";
                }
                ViewData["NumTimes"] = NumberTweets;
            }
            //If app is unable to retrieve tweet from user
            catch (Exception)
            {
                ViewData[string.Format("Message{0}", 0)] = "Unable to retrieve tweets from this user.";
                ViewData["NumTimes"] = 1;
            }
            return View();
        }

        // This function will obtain an authorization token from the twitter api using basic auth   
        public async Task<string> GetAccessToken()
        {
            //httpClient class is used for downloading web content 
            var httpClient = new HttpClient();
            //Post request is made on the api
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.twitter.com/oauth2/token ");
            //The consumer key and consumer secret is added for authorization
            var customerInfo = Convert.ToBase64String(new UTF8Encoding()
                                      .GetBytes("tdHnm9ytsQ0Ry0QSCXykOh03G" + ":" + "6gLDfqSol66u4FuPSAfHxRmEtVZJ9OUO4vHGgy9HEglriLwQ8i"));
            request.Headers.Add("Authorization", "Basic " + customerInfo);
            request.Content = new StringContent("grant_type=client_credentials",
                                                    Encoding.UTF8, "application/x-www-form-urlencoded");
            //Json Object is obtained
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            //Token is obtained from Json
            dynamic item = JsonConvert.DeserializeObject<object>(json);
            return item["access_token"];
        }


        //This method utilize the acces token inorder to obtain the latest tweets from a user
        public async Task<IEnumerable<string>> GetTweets(string userName, int count, string accessToken = null)
        {
            //Call GetAccessToken method
            if (accessToken == null)
            {
                accessToken = await GetAccessToken();
            }

            //Get request using the access token is made for a specific twitter user.
            var requestUserTimeline = new HttpRequestMessage(HttpMethod.Get,
                string.Format("https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={1}&count={0}&trim_user=1&exclude_replies=1", count, userName));
            requestUserTimeline.Headers.Add("Authorization", "Bearer " + accessToken);
            var httpClient = new HttpClient();
            //Json Object is obtained
            HttpResponseMessage responseUserTimeLine = await httpClient.SendAsync(requestUserTimeline);
            dynamic json = JsonConvert.DeserializeObject<object>(await responseUserTimeLine.Content.ReadAsStringAsync());
            var enumerableTweets = (json as IEnumerable<dynamic>);

            //If tweets are availible, then search for tweet text in json.
            if (enumerableTweets == null)
            {
                return null;
            }
                 return enumerableTweets.Select(t => (string)(t["text"].ToString()));   
        }
    }
}
