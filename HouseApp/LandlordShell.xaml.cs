using HouseApp.Views;

namespace HouseApp
{
    public partial class LandlordShell : Shell
    {
        public LandlordShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("housemanagement", typeof(HouseManagementPage));
            Routing.RegisterRoute("payment", typeof(PaymentPage));
        }
    }
}
