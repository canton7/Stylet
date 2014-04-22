using Stylet.Samples.RedditBrowser.RedditApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.RedditBrowser.Events
{
    public class OpenSubredditEvent
    {
        public string Subreddit { get; set; }
        public SortMode SortMode { get; set; }
    }
}
