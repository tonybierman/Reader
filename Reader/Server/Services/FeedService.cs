using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using Reader.Shared;
using Reader.Server.Controllers;

namespace Reader.Server.Services
{
    public class FeedService
    {
        readonly HttpClient _client;
        readonly ILogger<object>? _logger;

        //readonly ILocalStorage _localStorage;

        public FeedService(HttpClient httpClient)
        {
            _client = httpClient;
        }

        public FeedService(HttpClient httpClient, ILogger<object> logger) : this(httpClient)
        {
            _logger = logger;
        }

        //public FeedService(HttpClient httpClient, ILocalStorage localStorage, ILogger<FeedService> logger)
        //{
        //    _client = httpClient;
        //    _logger = logger;
        //    _localStorage = localStorage;
        //}

        //public async Task<List<Feed>> GetFeeds()
        //{
        //    var feeds = await _localStorage.GetItem<List<Feed>>("blazor.rss.feeds") ?? new List<Feed>();
        //    return feeds;
        //}

        //public async Task<Feed> GetFeedDetails(string feedId)
        //{
        //    var feeds = await GetFeeds();
        //    var feed = feeds.SingleOrDefault(f => f.Id.ToString() == feedId);
        //    return feed;
        //}
        public async Task<Feed?> GetFeedMetadata(string feedUrl)
        {
            try
            {
                Feed? feed = null;
                SyndicationFeed? syndicationFeed = await GetSyndicationFeed(feedUrl);
                if (syndicationFeed != null)
                {
                    feed = new Feed
                    {
                        Id = Guid.NewGuid(),
                        Description = syndicationFeed.Description?.Text,
                        ImageUrl = syndicationFeed.ImageUrl?.AbsoluteUri,
                        Items = syndicationFeed.Items.Take(3).Select(i => new FeedItem
                        {
                            Title = i.Title.Text,
                            Description = i.Summary.Text,
                            Url = i.Links[0].Uri.AbsoluteUri
                        }).ToList(),
                        Title = syndicationFeed.Title?.Text,
                        Url = feedUrl,
                        WebsiteUrl = syndicationFeed.Links.SingleOrDefault(l => l.RelationshipType == "alternate")?.Uri.AbsoluteUri,
                        LastUpdate = syndicationFeed.LastUpdatedTime.DateTime
                    };
                }
                return feed;
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex, "Error Parsing Feed");
                throw;
            }
        }

        //public async Task<List<RssSchema>> GetFeedItems(Feed selectedFeed)
        //{
        //    try
        //    {
        //        var rawContent = await GetRawContent(selectedFeed.Url);
        //        var feedContent = GetFeedContent(rawContent);
        //        List<RssSchema> feedItems = new RssParser().Parse(feedContent).ToList();
        //        return feedItems;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogCritical(ex, "Error Parsing Feed", null);
        //        throw;
        //    }
        //}

        //public async Task AddFeed(Feed feed)
        //{
        //    await Task.Run(async () =>
        //    {
        //        feed.Items = null;
        //        var existingFeeds = await GetFeeds();
        //        existingFeeds.Add(feed);
        //        await _localStorage.SetItem("blazor.rss.feeds", existingFeeds);
        //    }).ConfigureAwait(false);
        //}

        //public async Task  UpdateFeed(string feedId, string feedTitle)
        //{
        //    var feeds = await GetFeeds();
        //    var feed = feeds.SingleOrDefault(f => f.Id.ToString() == feedId);
        //    feed.Title = feedTitle;
        //    await _localStorage.SetItem("blazor.rss.feeds", feeds);
        //}

        //public async Task DeleteFeed(Guid feedId)
        //{
        //    await Task.Run(async () =>
        //    {
        //        var existingFeeds = await GetFeeds();
        //        var feedToDelete = existingFeeds.SingleOrDefault(f => f.Id == feedId);
        //        existingFeeds.Remove(feedToDelete);
        //        await _localStorage.SetItem("blazor.rss.feeds", existingFeeds);
        //    }).ConfigureAwait(false);
        //}

        //public async Task<SyndicationFeed> GetSyndicationFeed(string feedUrl)
        //{
        //    try
        //    {
        //        XmlReader reader = XmlReader.Create(feedUrl);
        //        var syndicationFeed = SyndicationFeed.Load(reader);
        //        return syndicationFeed;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message, new[] { feedUrl });
        //    }

        //    return null;
        //}

        public async Task<SyndicationFeed?> GetSyndicationFeed(String url)
        {
            var task = Task.Factory.StartNew(() =>
            {
                XmlReader reader = XmlReader.Create(url);
                return SyndicationFeed.Load(reader);
            });

            int timeout = 5000;
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {
                return await task;
            }
            else
            {
                return null;
            }
        }

        //private string GetFeedContent(string rawContent)
        //{
        //    XmlReader reader = XmlReader.Create(url);
        //    SyndicationFeed feed = SyndicationFeed.Load(reader);
        //    reader.Close();
        //    foreach (SyndicationItem item in feed.Items)
        //    {
        //        String subject = item.Title.Text;
        //        String summary = item.Summary.Text;
        //    }

        //    var xDoc = XDocument.Load(new StringReader(rawContent));
        //    var node = xDoc.XPathSelectElement("//query/results/*[1]");
        //    var reader = node.CreateReader();
        //    reader.MoveToContent();
        //    var feedString = reader.ReadOuterXml();
        //    return feedString;
        //}

        private async Task<string> GetRawContent(string feedUrl)
        {
            //var url = $"https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20xml%20where%20url%20%3D%20'{feedUrl}'&format=xml";
            var rawContent = await _client.GetStringAsync(feedUrl);
            return rawContent;
        }
    }
}
