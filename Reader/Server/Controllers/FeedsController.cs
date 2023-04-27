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
        public IEnumerable<Feed> Get(string cat)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };

            List<FeedConfiguration?> confList = new List<FeedConfiguration?>();

            string[] fileEntries = Directory.GetFiles($"var/{cat}", "*.json");
            foreach (var fname in fileEntries)
            {
                string jsonString = System.IO.File.ReadAllText(fname);
                FeedConfiguration? gp = JsonSerializer.Deserialize<FeedConfiguration>(jsonString, options)!;
                if (gp == null)
                    throw new NullReferenceException();
                confList.Add(gp);
            }

            List<Feed> feeds = new List<Feed>();
            foreach (var conf in confList)
            {
                if (conf?.FeedUrls != null)
                {
                    foreach (string url in conf.FeedUrls)
                    {
                        Feed f = new Feed();
                        f.Url = url;
                        feeds.Add(f);
                    }
                }
            }

            return feeds.ToArray();
        }
    }
}