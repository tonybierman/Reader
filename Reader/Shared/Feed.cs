﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Shared
{
    public class Feed
    {
        public Feed()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? ImageUrl { get; set; } = null;
        public DateTime LastUpdate { get; set; }
        public List<FeedItem>? Items { get; set; }
    }
}
