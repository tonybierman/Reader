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