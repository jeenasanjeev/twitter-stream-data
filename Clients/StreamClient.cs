using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Timers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TweetsCountRealData.Clients
{
    public class StreamClient
    {
        private string _ConsumerKey = "";
        private string _ConsumerSecret = "";
        private string _BearerToken = "";
        private string _streamEndpoint = "";
        private static Timer timer;
        public static int recordsFetch = 0;
        public ILogger<object> _logger;

        public event EventHandler StreamDataReceivedEvent;
        public class TweetReceivedEventArgs : EventArgs
        {
            public string StreamDataResponse { get; set; }
        }
        protected void OnStreamDataReceivedEvent(TweetReceivedEventArgs dataReceivedEventArgs)
        {
            if (StreamDataReceivedEvent == null)
                return;
            StreamDataReceivedEvent(this, dataReceivedEventArgs);
        }
        public StreamClient(string endPoint, string key,string secret, string token, ILogger<object> logger)
        {
            _streamEndpoint = endPoint;
            _ConsumerKey = key;
            _ConsumerSecret = secret;
            _BearerToken = token;
            _logger = logger;
        }


        public async Task StartStream(string address,  int maxConnectionAttempts)
        {
            
            int maxTries = maxConnectionAttempts;
            int tried = 0;
            int requestCount = 0;

            while (tried < maxTries)
            {
                tried++;
                try
                {
                    _logger.LogInformation($"Entered SampledStream at: {DateTime.Now.ToString("F")}");
                    //int recordsFetch = 0;

                    WebRequest request = WebRequest.Create(address);
                    request.Headers.Add("Authorization", "Bearer " + _BearerToken);
                    request.Method = "GET";
                    request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        requestCount++;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            //stream opened!
                            using (StreamReader str = new StreamReader(response.GetResponseStream()))
                            {

                                timer = new System.Timers.Timer();
                                timer.Interval = 300000;
                                timer.Elapsed += OnTimedEvent;
                                timer.AutoReset = true;
                                timer.Enabled = true;
                                // loop through each item in the Filtered Stream API
                                do
                                {
                                    string json = str.ReadLine();

                                    if (!string.IsNullOrEmpty(json))
                                    {
                                        // raise an event for a potential client to know we recieved data
                                        OnStreamDataReceivedEvent(new TweetReceivedEventArgs { StreamDataResponse = json });
                                        recordsFetch = recordsFetch + 1;

                                        
                                    }
                                } while (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                                            && !str.EndOfStream);
                            }
                            _logger.LogInformation($"Exited SampledStream at: {DateTime.Now.ToString("F")}");
                        }
                        else
                        {
                            _logger.LogInformation($"response.StatusCode not HttpStatusCode.OK. Currently: {response.StatusCode}  {response.StatusDescription}");
                        }
                    }
                    catch (WebException ex)
                    {
                        _logger.LogInformation($"Webexception Occured: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Something more serious happened. like for example you don't have network access
                        // we cannot talk about a server exception here as the server probably was never reached
                        _logger.LogInformation($"Exception Occured: {ex.Message}");
                    }
                    //we double-check the tries here just so if we aren't "trying" again we don't unnecessarily wait a few seconds
                    if (tried < maxTries)
                        System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    if (tried < maxTries)
                        System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(10));
                     _logger.LogInformation($"Exception Occured: {ex.Message}");
                }
            }
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Raised: {0}", e.SignalTime);
            Console.WriteLine("Total number of tweets received:" + recordsFetch + " at " + DateTime.Now.ToString("F"));
            Console.WriteLine("Average tweets per minute:" + (recordsFetch * 60000) / 300000);
        }
    }
}
