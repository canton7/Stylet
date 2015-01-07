using Stylet.Samples.RedditBrowser.RedditApi;
using System;

namespace Stylet.Samples.RedditBrowser.Events
{
    public class OpenSubredditEvent
    {
        public string Subreddit { get; set; }
        public SortMode SortMode { get; set; }
    }
}
