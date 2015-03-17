using Stylet.Samples.RedditBrowser.RedditApi;
using System;

namespace Stylet.Samples.RedditBrowser.Pages
{
    public class PostCommentsViewModel : Screen
    {
        private IRedditClient client;

        public string Subreddit { get; set; }
        public string PostId36 { get; set; }
        public CommentCollection CommentCollection { get; private set; }

        public PostCommentsViewModel(IRedditClient client)
        {
            this.client = client;
        }

        protected override async void OnInitialActivate()
        {
            this.CommentCollection = await this.client.GetPostComments(this.Subreddit, this.PostId36);
        }

        public void GoBack()
        {
            this.RequestClose();
        }
    }
}
