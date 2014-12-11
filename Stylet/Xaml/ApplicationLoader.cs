using System;
using System.Windows;

namespace Stylet.Xaml
{
    /// <summary>
    /// Added to your App.xaml, this is responsible for loading the Boostrapper you specify, and Stylet's other resources
    /// </summary>
    public class ApplicationLoader : ResourceDictionary
    {
        private readonly ResourceDictionary styletResourceDictionary;

        /// <summary>
        /// Create a new ApplicationLoader instance
        /// </summary>
        public ApplicationLoader()
        {
            this.styletResourceDictionary = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Stylet;component/Xaml/StyletResourceDictionary.xaml", UriKind.Absolute) };
            this.LoadStyletResources = true;
        }

        private IBootstrapper _bootstrapper;

        /// <summary>
        /// Bootstrapper instance to use to start your application. This must be set.
        /// </summary>
        public IBootstrapper Bootstrapper
        {
            get { return _bootstrapper; }
            set
            {
                this._bootstrapper = value;
                this._bootstrapper.Setup(Application.Current);
            }
        }

        private bool _loadStyletResources;

        /// <summary>
        /// Control whether to load Stylet's own resources (e.g. StyletConductorTabControl). Defaults to true.
        /// </summary>
        public bool LoadStyletResources
        {
            get { return this._loadStyletResources; }
            set
            {
                this._loadStyletResources = value;
                if (this._loadStyletResources)
                    this.MergedDictionaries.Add(this.styletResourceDictionary);
                else
                    this.MergedDictionaries.Remove(this.styletResourceDictionary);
            }
        }
    }
}
