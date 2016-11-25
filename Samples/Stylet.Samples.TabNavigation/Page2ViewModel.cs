using System;

namespace Stylet.Samples.TabNavigation
{
    class Page2ViewModel : Screen, IDisposable
    {
        public Page2ViewModel()
        {
            this.DisplayName = "Page 2";
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }

        protected override void OnClose()
        {
            base.OnClose();
        }

        public void Dispose()
        {

        }
    }
}
