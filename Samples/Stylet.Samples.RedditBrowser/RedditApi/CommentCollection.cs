using Spring.Rest.Client;
using Stylet.Samples.RedditBrowser.RedditApi.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.RedditBrowser.RedditApi
{
    public class CommentCollection
    {
        private RestTemplate template;
        private string subreddit;
        private string postId;

        public IReadOnlyList<Comment> Comments { get; private set; }

        public CommentCollection(RestTemplate template, string subreddit, string postId)
        {
            this.template = template;
            this.subreddit = subreddit;
            this.postId = postId;
        }

        public async Task LoadAsync()
        {
            var comments = await this.template.GetForObjectAsync<List<CommentsResponse>>("/r/{subreddit}/comments/{postid}.json", this.subreddit, this.postId);
            this.Comments = comments.SelectMany(x => x.Data.Children).Where(x => x.Kind == "t1").Select(x => this.ContractToComment(x.Data)).ToList();
        }

        private Comment ContractToComment(CommentData data)
        {
            var comment = new Comment()
            {
                Body = data.Body,
                Author = data.Author
            };
            if (data.Replies != null && data.Replies.Data != null)
                comment.Replies = data.Replies.Data.Children.Select(x => this.ContractToComment(x.Data)).ToList();
            return comment;
        }
    }

    public class Comment
    {
        public string Body { get; set; }
        public string Author { get; set; }
        public List<Comment> Replies { get; set; }
    }
}
