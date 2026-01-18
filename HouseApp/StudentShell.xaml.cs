using HouseApp.Views;

namespace HouseApp
{
    public partial class StudentShell : Shell
    {
        public StudentShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("housemanagement", typeof(HouseManagementPage));
            Routing.RegisterRoute("payment", typeof(PaymentPage));
        }
    }
}
