using Microsoft.AspNetCore.Mvc;
using Reader.Server.Services;
using Reader.Shared;
using System.ServiceModel.Syndication;
using System.Text.Json;

namespace Reader.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedsController : ControllerBase
    {
        private readonly ILogger<FeedsController> _logger;

        public FeedsController(ILogger<FeedsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Feed> Get()
        {
            string fname = Path.Combine(Environment.CurrentDirectory, "var/feeds.json");
            string jsonString = System.IO.File.ReadAllText(fname);
            FeedConfiguration conf = JsonSerializer.Deserialize<FeedConfiguration>(jsonString)!;
            if (conf == null)
                throw new NullReferenceException();

            List<Feed> feeds = new List<Feed>();
            foreach (string url in conf.FeedUrls)
            {
                Feed f = new Feed();
                f.Url = url;
                feeds.Add(f);
            }

            return feeds.ToArray();
        }
    }
}