using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestEase;
using Stylet.Samples.RedditBrowser.RedditApi.Contracts;

namespace Stylet.Samples.RedditBrowser.RedditApi
{
    public interface IRedditApi
    {
        [Get("/r/{subreddit}/{mode}.json")]
        Task<PostsResponse> FetchPostsAsync([Path] string subreddit, [Path] string mode);

        [Get("/r/{subreddit}/{mode}.json?after={after}&count={count}")]
        Task<PostsResponse> FetchNextPostsAsync([Path] string subreddit, [Path] string mode, [Path] string after, [Path] int count);

        [Get("/r/{subreddit}/{mode}.json?before={before}&count={count}")]
        Task<PostsResponse> FetchPrevPostsAsync([Path] string subreddit, [Path] string mode, [Path] string before, [Path] int count);

        [Get("/r/{subreddit}/comments/{postId}.json")]
        Task<List<CommentsResponse>> FetchCommentsAsync([Path] string subreddit, [Path] string postId);
    }
}
