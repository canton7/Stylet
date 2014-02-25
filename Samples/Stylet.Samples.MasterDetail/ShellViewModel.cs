using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.MasterDetail
{
    public class ShellViewModel : Screen
    {
        public IObservableCollection<EmployeeModel> Employees { get; private set; }

        private EmployeeModel _selectedEmployee;
        public EmployeeModel SelectedEmployee
        {
            get { return this._selectedEmployee; }
            set
            {
                this._selectedEmployee = value;
                this.NotifyOfPropertyChange();
            }
        }

        public ShellViewModel()
        {
            this.DisplayName = "Master-Detail";

            this.Employees = new BindableCollection<EmployeeModel>();

            this.Employees.Add(new EmployeeModel() { Name = "Fred" });
        }

        public void RemoveItem(EmployeeModel item)
        {
            this.Employees.Remove(item);
        }
    }
}
