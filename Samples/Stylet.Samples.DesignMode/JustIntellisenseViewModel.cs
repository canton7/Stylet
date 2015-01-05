using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.DesignMode
{
    public class JustIntellisenseViewModel : Screen
    {
        public string TextBoxText { get; set; }

        public JustIntellisenseViewModel()
        {
            this.TextBoxText = "This text is not displayed in the View";
        }
    }
}
