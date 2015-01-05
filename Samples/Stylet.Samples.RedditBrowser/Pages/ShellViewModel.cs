using Stylet.Samples.RedditBrowser.Events;
using System;

namespace Stylet.Samples.RedditBrowser.Pages
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<OpenSubredditEvent>
    {
        private ISubredditViewModelFactory subredditViewModelFactory;

        public TaskbarViewModel Taskbar { get; private set; }

        public ShellViewModel(IEventAggregator events, TaskbarViewModel taskbarViewModel, ISubredditViewModelFactory subredditViewModelFactory)
        {
            this.DisplayName = "Reddit Browser";

            this.Taskbar = taskbarViewModel;
            this.subredditViewModelFactory = subredditViewModelFactory;

            events.Subscribe(this);
        }

        public void Handle(OpenSubredditEvent message)
        {
            var item = this.subredditViewModelFactory.CreateSubredditViewModel();
            item.Subreddit = message.Subreddit;
            item.SortMode = message.SortMode;
            this.ActivateItem(item);
        }
    }

    public interface ISubredditViewModelFactory
    {
        SubredditViewModel CreateSubredditViewModel();
    }
}
