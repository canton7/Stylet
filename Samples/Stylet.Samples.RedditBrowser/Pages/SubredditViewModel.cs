using Stylet.Samples.RedditBrowser.RedditApi;
using System;

namespace Stylet.Samples.RedditBrowser.Pages
{
    public class SubredditViewModel : Conductor<IScreen>.StackNavigation
    {
        private IPostCommentsViewModelFactory postCommentsViewModelFactory;

        public string Subreddit { get; set; }
        public SortMode SortMode { get; set; }
        private PostsViewModel posts;

        public SubredditViewModel(PostsViewModel posts, IPostCommentsViewModelFactory postCommentsViewModelFactory)
        {
            this.posts = posts;
            this.postCommentsViewModelFactory = postCommentsViewModelFactory;

            this.posts.PostCommentsOpened += (o, e) => this.OpenPostComments(e.PostId36);
        }

        protected override void OnInitialActivate()
        {
            this.DisplayName = String.Format("/r/{0}", this.Subreddit);
            this.posts.Subreddit = this.Subreddit;
            this.posts.SortMode = this.SortMode;

            this.ActivateItem(this.posts);
        }

        private void OpenPostComments(string postId36)
        {
            var item = this.postCommentsViewModelFactory.CreatePostCommentsViewModel();
            item.PostId36 = postId36;
            item.Subreddit = this.Subreddit;
            this.ActivateItem(item);
        }

        protected override void ChangeActiveItem(IScreen newItem, bool closePrevious)
        {
            base.ChangeActiveItem(newItem, closePrevious);

            // If we're setting the ActiveItem to null, that means everything's been closed - close ourselves
            if (newItem == null)
                this.RequestClose();
        }
    }

    public interface IPostCommentsViewModelFactory
    {
        PostCommentsViewModel CreatePostCommentsViewModel();
    }
}
