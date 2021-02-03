﻿using StyletIoC;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Stylet
{
    /// <summary>
    /// Bootstrapper to be extended by any application which wants to use StyletIoC (the default)
    /// </summary>
    /// <typeparam name="TRootViewModel">Type of the root ViewModel. This will be instantiated and displayed</typeparam>
    public abstract class Bootstrapper<TRootViewModel> : BootstrapperNoRootView where TRootViewModel : class
    {
        private TRootViewModel _rootViewModel;

        /// <summary>
        /// Gets the root ViewModel, creating it first if necessary
        /// </summary>
        protected virtual TRootViewModel RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.Container.Get<TRootViewModel>()); }
        }
        
        /// <summary>
        /// Called when the application is launched. Displays the root view.
        /// </summary>
        protected override void Launch()
        {
            this.DisplayRootView(this.RootViewModel);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // Dispose the container last
            base.Dispose();
            // Don't create the root ViewModel if it doesn't already exist...
            ScreenExtensions.TryDispose(this._rootViewModel);
            if (this.Container != null)
                this.Container.Dispose();
        }
    }
}
