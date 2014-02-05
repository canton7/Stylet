using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet
{
    public partial class Conductor<T> : ConductorBaseWithActiveItem<T> where T : class
    {
        public override async void ActivateItem(T item)
        {
            if (item != null && item.Equals(this.ActiveItem))
            {
                if (this.IsActive)
                    ScreenExtensions.TryActivate(item);
            }
            else if (await this.CanCloseItem(this.ActiveItem))
            {
                this.ChangeActiveItem(item, true);
            }
        }

        public override async void DeactivateItem(T item, bool close)
        {
            if (item == null || !item.Equals(this.ActiveItem))
                return;

            if (await this.CanCloseItem(item))
                this.ChangeActiveItem(default(T), close);
        }

        public override Task<bool> CanClose()
        {
            return this.CanCloseItem(this.ActiveItem);
        }
    }
}
