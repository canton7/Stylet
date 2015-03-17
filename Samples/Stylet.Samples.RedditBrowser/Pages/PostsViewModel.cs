using Stylet.Samples.RedditBrowser.RedditApi;
using System;
using System.Diagnostics;

namespace Stylet.Samples.RedditBrowser.Pages
{
    public class PostsViewModel : Screen
    {
        public event EventHandler<PostCommentsOpenedEventArgs> PostCommentsOpened;

        private IRedditClient client;

        public string Subreddit { get; set; }
        public SortMode SortMode { get; set; }
        public PostCollection PostCollection { get; private set; }
        public Post SelectedPost { get; set; }

        public PostsViewModel(IRedditClient client)
        {
            this.client = client;
        }

        protected override async void OnInitialActivate()
        {
            // Really un-advanced - just close
            try
            {
                this.PostCollection = await this.client.GetPostsAsync(this.Subreddit, this.SortMode);
            }
            catch (Exception)
            {
                this.RequestClose();
            }
        }

        public void Close()
        {
            this.RequestClose();
        }

        public bool CanNext
        {
            get { return this.PostCollection != null && this.PostCollection.HasNext; }
        }
        public async void Next()
        {
            this.PostCollection = await this.PostCollection.NextAsync();
        }

        public bool CanPrev
        {
            get { return this.PostCollection != null && this.PostCollection.HasPrev; }
        }
        public async void Prev()
        {
            this.PostCollection = await this.PostCollection.PrevAsync();
        }

        public bool CanOpenSelected
        {
            get { return this.SelectedPost != null; }
        }
        public void OpenSelected()
        {
            if (this.SelectedPost == null)
                return;

            Process.Start(this.SelectedPost.LinkUrl);
        }

        public bool CanOpenSelectedComments
        {
            get { return this.SelectedPost != null && this.SelectedPost.NumComments > 0; }
        }
        public void OpenSelectedComments()
        {
            if (this.SelectedPost == null)
                return;

            var handler = this.PostCommentsOpened;
            if (handler != null)
                handler(this, new PostCommentsOpenedEventArgs() { PostId36 = this.SelectedPost.Id36 });
        }
    }

    public class PostCommentsOpenedEventArgs : EventArgs
    {
        public string PostId36 { get; set; }
    }
}
