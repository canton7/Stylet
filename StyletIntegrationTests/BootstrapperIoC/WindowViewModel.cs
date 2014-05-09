using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests.BootstrapperIoC
{
    interface BootstrapperIoCI1 { }
    class BootstrapperIoCC1 : BootstrapperIoCI1 { }
    class BootstrapperIoCC2 : BootstrapperIoCI1 { }
    class BootstrapperIoCC3
    {
        [Inject]
        public BootstrapperIoCC1 C1 { get; set; }
    }

    public class WindowViewModel : Screen
    {
        public void GetSingle()
        {
            var result = IoC.Get<BootstrapperIoCC1>();
            if (result == null)
                throw new Exception("IoC.Get failed");
        }

        public void GetAll()
        {
            var result = IoC.GetAll<BootstrapperIoCI1>().ToList();
            if (result.Count != 2 || !(result[0] is BootstrapperIoCC1) || !(result[1] is BootstrapperIoCC2))
                throw new Exception("IoC.GetAll failed");
        }

        public void BuildUp()
        {
            var c3 = new BootstrapperIoCC3();
            IoC.BuildUp(c3);
            if (c3.C1 == null)
                throw new Exception("IoC.BuildUp failed");
        }
    }
}
