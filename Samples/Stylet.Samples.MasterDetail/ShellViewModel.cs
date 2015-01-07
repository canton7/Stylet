using System;
using System.Linq;

namespace Stylet.Samples.MasterDetail
{
    public class ShellViewModel : Screen
    {
        public IObservableCollection<EmployeeModel> Employees { get; private set; }

        private EmployeeModel _selectedEmployee;
        public EmployeeModel SelectedEmployee
        {
            get { return this._selectedEmployee; }
            set { SetAndNotify(ref this._selectedEmployee, value) ;}
        }

        public ShellViewModel()
        {
            this.DisplayName = "Master-Detail";

            this.Employees = new BindableCollection<EmployeeModel>();

            this.Employees.Add(new EmployeeModel() { Name = "Fred" });
            this.Employees.Add(new EmployeeModel() { Name = "Bob" });

            this.SelectedEmployee = this.Employees.FirstOrDefault();
        }

        public void AddEmployee()
        {
            this.Employees.Add(new EmployeeModel() { Name = "Unnamed" });
        }

        public void RemoveEmployee(EmployeeModel item)
        {
            this.Employees.Remove(item);
        }
    }
}
