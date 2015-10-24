using Stylet.Samples.RedditBrowser.RedditApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stylet.Samples.RedditBrowser.RedditApi
{
    public class PostCollection
    {
        private IRedditApi api;
        private string subreddit;
        private string sortMode;
        private string after;
        private string before;
        /// <summary>
        /// The number of items we've seen in this listing, *INCLUDING* the current posts
        /// </summary>
        private int count;

        public IReadOnlyList<Post> Posts { get; private set; }

        public bool HasNext
        {
            get { return this.after != null; }
        }
        public bool HasPrev
        {
            get { return this.before != null; }
        }

        public PostCollection(IRedditApi api, string subreddit, string sortMode)
            : this(api, subreddit, sortMode, 0) { }

        private PostCollection(IRedditApi api, string subreddit, string sortMode, int count)
        {
            this.api = api;
            this.subreddit = subreddit;
            this.sortMode = sortMode;
            this.count = count;
        }

        public async Task LoadAsync()
        {
            var posts = await this.api.FetchPostsAsync(this.subreddit, this.sortMode);
            this.LoadPostsResponse(posts);
            this.count = this.Posts.Count;
        }

        private void LoadPostsResponse(PostsResponse posts)
        {
            this.Posts = posts.Data.Children.Select(x => new Post()
            {
                Title = x.Data.Title,
                LinkUrl = x.Data.Url,
                Id36 = x.Data.Id,
                NumComments = x.Data.NumComments,
            }).ToList();
            this.after = posts.Data.After;
            this.before = posts.Data.Before;
        }

        public async Task<PostCollection> NextAsync()
        {
            var posts = await this.api.FetchNextPostsAsync(this.subreddit, this.sortMode, this.after, this.count);
            var result = new PostCollection(this.api, this.subreddit, this.sortMode, this.count + posts.Data.Children.Count);
            result.LoadPostsResponse(posts);
            return result;
        }

        public async Task<PostCollection> PrevAsync()
        {
            var posts = await this.api.FetchPrevPostsAsync(this.subreddit, this.sortMode, this.before, this.count);
            var result = new PostCollection(this.api, this.subreddit, this.sortMode, this.count - posts.Data.Children.Count);
            result.LoadPostsResponse(posts);
            return result;
        }
    }

    public class Post
    {
        public string Title { get; set; }
        public string LinkUrl { get; set; }
        public string Id36 { get; set; }
        public int NumComments { get; set; }
    }
}
