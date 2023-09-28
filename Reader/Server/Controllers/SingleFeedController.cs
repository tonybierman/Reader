/*
 * Author: Tony Bierman
 * Website: http://www.tonybierman.com
 * License: MIT License
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
 * IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
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
        public async Task<Feed?> Get(string url, string length)
        {
            int timeout = 5000;
            int l = Convert.ToInt32(length);
            if (l == 0)
                l = 5;

            var task = LoadFeed(url, l);
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return task.Result;
            }

            return null;
        }

        private async Task<Feed> LoadFeed(string url, int length)
        {
            Feed feed = new Feed() { Id = Guid.Empty };
            FeedService svc = new FeedService(new HttpClient(), _logger);
            SyndicationFeed? syndicationFeed = await svc.GetSyndicationFeed(url);

            if (syndicationFeed != null)
            {
                feed = new Feed
                {
                    Id = Guid.NewGuid(),
                    Description = syndicationFeed.Description?.Text,
                    ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
                    Items = syndicationFeed.Items.OrderByDescending(a => a.PublishDate).Take(length).Select(i => new FeedItem
                    {
                        Title = i.Title.Text,
                        Description = i.Summary.Text,
                        Url = i.Links[0].Uri.AbsoluteUri,
                        Published = i.PublishDate.LocalDateTime
                    }).ToList(),
                    Url = url,
                    Title = syndicationFeed.Title?.Text,
                    WebsiteUrl = syndicationFeed.Links.SingleOrDefault(l => l.RelationshipType == "alternate")?.Uri.AbsoluteUri,
                    LastUpdate = syndicationFeed.LastUpdatedTime.DateTime
                };
            }

            return feed;
        }
    }
}