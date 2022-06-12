using Microsoft.AspNetCore.Mvc;
using Reader.Server.Services;
using Reader.Shared;
using System.ServiceModel.Syndication;

namespace Reader.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SingleFeedController : ControllerBase
    {
        private readonly ILogger<SingleFeedController> _logger;

        private static readonly string[] Summaries = new[]
        {
            @"https://moxie.foxnews.com/feedburner/latest.xml",
            @"https://moxie.foxnews.com/feedburner/tech.xml",
            @"https://moxie.foxnews.com/feedburner/opinion.xml"
        };

        public SingleFeedController(ILogger<SingleFeedController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Feed Get(string url)
        {
            Task<Feed> f = LoadFeed(url);

            return f.Result;
        }

        private async Task<Feed> LoadFeed(string url)
        {
            FeedService svc = new FeedService(new HttpClient(), _logger);

            SyndicationFeed syndicationFeed = await svc.GetSyndicationFeed(url);

            if (syndicationFeed != null)
            {
                Feed feed = new Feed
                {
                    Id = Guid.NewGuid(),
                    Description = syndicationFeed.Description?.Text,
                    ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
                    Items = syndicationFeed.Items.Take(8).Select(i => new FeedItem
                    {
                        Title = i.Title.Text,
                        Description = i.Summary.Text,
                        Url = i.Links[0].Uri.AbsoluteUri
                    }).ToList(),
                    Url = url,
                    Title = syndicationFeed.Title?.Text,
                    WebsiteUrl = syndicationFeed.Links.SingleOrDefault(l => l.RelationshipType == "alternate")?.Uri.AbsoluteUri,
                    LastUpdate = syndicationFeed.LastUpdatedTime.DateTime
                };

                return feed;
            }
            else
                return null;
        }
    }
}