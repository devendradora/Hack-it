﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace Hack_it
{
   // FeedData
    // Holds info for a single blog feed, including a list of blog posts (FeedItem).
    public class FeedData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }

        private List<FeedItem> _Items = new List<FeedItem>();
        public List<FeedItem> Items
        {
            get
            {
                return this._Items;
            }
        }
    }

    // FeedItem
    // Holds info for a single blog post.
    public class FeedItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime PubDate { get; set; }
        public Uri Link { get; set; }
    }

    // FeedDataSource
    // Holds a collection of blog feeds (FeedData), and contains methods needed to
    // retreive the feeds.
    public class FeedDataSource
    {
        private ObservableCollection<FeedData> _Feeds = new ObservableCollection<FeedData>();
        public ObservableCollection<FeedData> Feeds
        {
            get
            {
                return this._Feeds;
            }
        }

        public async Task GetFeedsAsync()
        {
            Task<FeedData> feed1 =
                GetFeedAsync("http://devendradora.blogspot.in/feeds/posts/default?alt=rss");
            Task<FeedData> feed2 =
                     GetFeedAsync("http://hackingprotectdevdora.blogspot.in/feeds/posts/default?alt=rss");
            Task<FeedData> feed3 =
                GetFeedAsync("http://programmingdevdora.blogspot.in/feeds/posts/default?alt=rss");
            Task<FeedData> feed4 =
                GetFeedAsync("http://artificialintelligencedevdora.blogspot.in/feeds/posts/default?alt=rss");

            this.Feeds.Add(await feed1);
            this.Feeds.Add(await feed2);
            this.Feeds.Add(await feed3);
            this.Feeds.Add(await feed4);

        }

        private async Task<FeedData> GetFeedAsync(string feedUriString)
        {
            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri(feedUriString);

            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                // This code is executed after RetrieveFeedAsync returns the SyndicationFeed.
                // Process the feed and copy the data you want into the FeedData and FeedItem classes.
                FeedData feedData = new FeedData();

                if (feed.Title != null && feed.Title.Text != null)
                {
                    feedData.Title = feed.Title.Text;
                }
                if (feed.Subtitle != null && feed.Subtitle.Text != null)
                {
                    feedData.Description = feed.Subtitle.Text;
                }
                if (feed.Items != null && feed.Items.Count > 0)
                {
                    // Use the date of the latest post as the last updated date.
                    feedData.PubDate = feed.Items[0].PublishedDate.DateTime;

                    foreach (SyndicationItem item in feed.Items)
                    {
                        FeedItem feedItem = new FeedItem();
                        if (item.Title != null && item.Title.Text != null)
                        {
                            feedItem.Title = item.Title.Text;
                        }
                        if (item.PublishedDate != null)
                        {
                            feedItem.PubDate = item.PublishedDate.DateTime;
                        }
                        if (item.Authors != null && item.Authors.Count > 0)
                        {
                            feedItem.Author = item.Authors[0].Name.ToString();
                        }
                        // Handle the differences between RSS and Atom feeds.
                        if (feed.SourceFormat == SyndicationFormat.Atom10)
                        {
                            if (item.Content != null && item.Content.Text != null)
                            {
                                feedItem.Content = item.Content.Text;
                            }
                            if (item.Id != null)
                            {
                                feedItem.Link = new Uri("http://windowsteamblog.com" + item.Id);
                            }
                        }
                        else if (feed.SourceFormat == SyndicationFormat.Rss20)
                        {
                            if (item.Summary != null && item.Summary.Text != null)
                            {
                                feedItem.Content = item.Summary.Text;
                            }
                            if (item.Links != null && item.Links.Count > 0)
                            {
                                feedItem.Link = item.Links[0].Uri;
                            }
                        }
                        feedData.Items.Add(feedItem);
                    }    
                }
                return feedData;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Returns the feed that has the specified title.
        public static FeedData GetFeed(string title)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;

            var matches = _feedDataSource.Feeds.Where((feed) => feed.Title.Equals(title));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        // Returns the post that has the specified title.
        public static FeedItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            var _feeds = _feedDataSource.Feeds;

            var matches = _feedDataSource.Feeds.SelectMany(group => group.Items).Where((item) => item.Title.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }
    }
}
