using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;


namespace Vertikal.Views;

public partial class SummitsPage : ContentPage
{
    private readonly IFirebaseSummitService _summitService;
    private List<Summits> _allSummits = new();
    private List<string> _provincias;

    public SummitsPage(IFirebaseSummitService summitService)
    {
        InitializeComponent();
        _summitService = summitService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _allSummits = await _summitService.GetSummitListAsync();
        _provincias = _allSummits
            .Select(s => s.Provincia)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .OrderBy(p => p)
            .ToList();
        _provincias.Insert(0, "Todas"); 
        ProvinciaPicker.ItemsSource = _provincias;
        ProvinciaPicker.SelectedIndex = 0; // Selecciona "Todas" por defecto
        SummitsCollectionView.ItemsSource = _allSummits;
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var filter = e.NewTextValue?.ToLower() ?? "";
        var filtered = string.IsNullOrWhiteSpace(filter)
            ? _allSummits
            : _allSummits.Where(s => s.Name?.ToLower().Contains(filter) == true).ToList();

        SummitsCollectionView.ItemsSource = filtered.OrderBy(s => s.Name).ToList();
    }


    private void OnProvinciaChanged(object sender, EventArgs e)
    {
        var selected = ProvinciaPicker.SelectedItem as string;
        if (string.IsNullOrEmpty(selected) || selected == "Todas")
            SummitsCollectionView.ItemsSource = _allSummits;
        else
            SummitsCollectionView.ItemsSource = _allSummits.Where(s => s.Provincia == selected).ToList();
    }

    private async void OnVerMapaClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Próximamente", "La funcionalidad de ver todas las montañas en el mapa estará disponible en futuras versiones.", "OK");
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}