using Vertikal.Core.Interfaces;
using Vertikal.ViewModel;

namespace Vertikal.Views;

public partial class HistorialPage : ContentPage
{

    private readonly IFirebaseAscentService _ascentService;
    private readonly IFirebaseSummitService _summitService;

    private string _userId;

    public HistorialPage( IFirebaseAscentService ascentService, IFirebaseSummitService summitService)
    {
        InitializeComponent();
        _ascentService = ascentService;
        _summitService = summitService;
        CargarAscensiones();
    }



    private async void CargarAscensiones()
    {
        var inicio = new DateTime(DateTime.Now.Year, 1, 1);
        var fin = DateTime.Now;
        _userId = await SecureStorage.GetAsync("FirebaseUid"); ;
        AscentsListView.ItemsSource = await GetAscentsWithSummitDataAsync(_userId, inicio, fin);
        
    }

    private async void OnFiltrarPorFechasClicked(object sender, EventArgs e)
    {
        var inicio = FechaInicioPicker.Date;
        var fin = FechaFinPicker.Date;
        AscentsListView.ItemsSource = await GetAscentsWithSummitDataAsync(_userId, inicio, fin);
    }


    public async Task<List<AscentHistoryViewModel>> GetAscentsWithSummitDataAsync(string userId, DateTime start, DateTime end)
    {
        var ascents = await _ascentService.GetAscentsByUserAndDateRangeAsync(userId, start, end);
        var allSummits = await _summitService.GetSummitListAsync();

        var result = ascents
            .Join(allSummits,
                  ascent => ascent.SummitId,
                  summit => summit.Id,
                  (ascent, summit) => new AscentHistoryViewModel
                  {
                      SummitName = summit.Name,
                      SummitAltitude = summit.Altitud,         
                      SummitDescription = summit.Description,
                      Date = ascent.Date,
                      ValidationMethod = ascent.ValidationMethod
                  })
            .OrderByDescending(x => x.Date)
            .ToList();

        return result;
    }


    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}