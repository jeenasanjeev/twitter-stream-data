using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TweetsCountRealData.Clients;

namespace TweetsCountRealData
{
    public class Runner
    {
        private readonly ILogger<object> logger;
        private readonly string twitter_API;
        private readonly string twitter_API_Key;
        private readonly string twitter_API_Secret;
        private readonly string twitter_API_Token;

        public Runner()
        {
            logger = ServiceProviderFactory.ServiceProvider.GetRequiredService<ILogger<object>>();
            twitter_API = ServiceProviderFactory.Configuration["General:Twitter_API"];
            twitter_API_Key = ServiceProviderFactory.Configuration["General:Twitter_API_Key"];
            twitter_API_Secret = ServiceProviderFactory.Configuration["General:Twitter_API_Secret"];
            twitter_API_Token = ServiceProviderFactory.Configuration["General:Twitter_API_Token"];
            
        }

        public async Task Execute()
        {
            try
            {
                logger.LogInformation($"Making connection to Twitter API using Bearer Token");
                StreamClient streamClient = new StreamClient(twitter_API, twitter_API_Key, twitter_API_Secret, twitter_API_Token, logger);
               
                logger.LogInformation($"Accessing Twitter data");
                await streamClient.StartStream("https://api.twitter.com/2/tweets/sample/stream?expansions=attachments.poll_ids,attachments.media_keys,author_id,entities.mentions.username,geo.place_id,in_reply_to_user_id,referenced_tweets.id,referenced_tweets.id.author_id", 5);
          
            }
            catch(Exception ex)
            {
                logger.LogInformation($"Exception Occured: {ex.Message}");
                logger.LogInformation($"Inner Exception Occured: {ex.InnerException}");
            }
        }

       
    }
}
