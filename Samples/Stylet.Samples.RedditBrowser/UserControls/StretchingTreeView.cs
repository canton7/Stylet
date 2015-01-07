using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Stylet.Samples.RedditBrowser.UserControls
{
    // http://blogs.msdn.com/b/jpricket/archive/2008/08/05/wpf-a-stretching-treeview.aspx
    public class StretchingTreeView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }
    }

    public class StretchingTreeViewItem : TreeViewItem
    {
        public StretchingTreeViewItem()
        {
            this.Loaded += new RoutedEventHandler(StretchingTreeViewItem_Loaded);
        }

        private void StretchingTreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            // The purpose of this code is to stretch the Header Content all the way accross the TreeView. 
            if (this.VisualChildrenCount > 0)
            {
                Grid grid = this.GetVisualChild(0) as Grid;
                if (grid != null && grid.ColumnDefinitions.Count == 3)
                {
                    // Remove the middle column which is set to Auto and let it get replaced with the 
                    // last column that is set to Star.
                    grid.ColumnDefinitions.RemoveAt(1);
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new StretchingTreeViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is StretchingTreeViewItem;
        }

        // http://stackoverflow.com/a/2957734/1086121
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                var btn = Template.FindName("Expander", this) as ToggleButton;
                if (btn != null)
                    btn.VerticalAlignment = this.ToggleButtonVerticalAlignment;
            }
        }



        public VerticalAlignment ToggleButtonVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(ToggleButtonVerticalAlignmentProperty); }
            set { SetValue(ToggleButtonVerticalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToggleButtonVerticalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToggleButtonVerticalAlignmentProperty =
            DependencyProperty.Register("ToggleButtonVerticalAlignment", typeof(VerticalAlignment), typeof(StretchingTreeViewItem), new PropertyMetadata(VerticalAlignment.Center));

        
    }
}
