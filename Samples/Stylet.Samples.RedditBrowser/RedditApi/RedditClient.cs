using Spring.Http.Converters.Json;
using Spring.Rest.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stylet.Samples.RedditBrowser.RedditApi
{
    public interface IRedditClient
    {
        Task<PostCollection> GetPostsAsync(string subreddit, SortMode sortMode);
        Task<CommentCollection> GetPostComments(string subreddit, string postId36);
    }

    public class RedditClient : IRedditClient
    {
        private RestTemplate template;

        public RedditClient()
        {
            this.template = new RestTemplate("http://reddit.com");
            template.MessageConverters.Add(new DataContractJsonHttpMessageConverter());
        }

        public async Task<PostCollection> GetPostsAsync(string subreddit, SortMode sortMode)
        {
            var postCollection = new PostCollection(this.template, subreddit, sortMode.Mode);
            await postCollection.LoadAsync();
            return postCollection;
        }

        public async Task<CommentCollection> GetPostComments(string subreddit, string postId36)
        {
            var commentCollection = new CommentCollection(this.template, subreddit, postId36);
            await commentCollection.LoadAsync();
            return commentCollection;
        }
    }

    public struct SortMode
    {
        public static SortMode New = new SortMode("new", "New");
        public static SortMode Hot = new SortMode("hot", "Hot");
        public static SortMode Top = new SortMode("top", "Top");

        public static IEnumerable<SortMode> AllModes
        {
            get { return new[] { New, Hot, Top }; }
        }

        public string Name { get; private set; }
        public string Mode { get; private set; }
        private SortMode(string mode, string name) : this()
        {
            this.Mode = mode;
            this.Name = name;
        }
    }
}
