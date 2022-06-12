using Microsoft.AspNetCore.Mvc;
using Reader.Server.Services;
using Reader.Shared;
using System.ServiceModel.Syndication;

namespace Reader.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedsController : ControllerBase
    {
        private readonly ILogger<FeedsController> _logger;

        private static readonly string[] Summaries = new[]
        {
            @"https://moxie.foxnews.com/feedburner/latest.xml",
            @"https://moxie.foxnews.com/feedburner/tech.xml",
            @"https://moxie.foxnews.com/feedburner/opinion.xml"
        };

        public FeedsController(ILogger<FeedsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Feed> Get()
        {
            List<Feed> feeds = new List<Feed>();
            foreach (string url in Summaries)
            {
                Feed f = new Feed();
                f.Url = url;
                feeds.Add(f);
            }

            return feeds.ToArray();
        }
    }
}