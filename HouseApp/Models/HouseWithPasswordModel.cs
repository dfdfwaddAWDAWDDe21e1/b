using CommunityToolkit.Mvvm.ComponentModel;

namespace HouseApp.Models;

public partial class HouseWithPasswordModel : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private int landlordId;

    [ObservableProperty]
    private decimal monthlyRent;

    [ObservableProperty]
    private decimal utilitiesCost;

    [ObservableProperty]
    private decimal waterBillCost;

    [ObservableProperty]
    private DateTime createdDate;

    [ObservableProperty]
    private int maxOccupants;

    [ObservableProperty]
    private int currentOccupants;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private int availableSpots;
}
