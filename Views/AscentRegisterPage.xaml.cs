using Vertikal.Core.Models;
using Vertikal.Core.Interfaces;

namespace Vertikal.Views;


public partial class AscentRegisterPage : ContentPage
{
    private readonly IFirebaseSummitService _summitService;
    private readonly IFirebaseAscentService _ascentService;
 

    public AscentRegisterPage(IFirebaseSummitService summitService, IFirebaseAscentService ascentService)
	{
		InitializeComponent();
        _summitService = summitService;
        _ascentService = ascentService;
    
    }


    private async void OnRegistrarAutomaticoClicked(object sender, EventArgs e)
    {
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));



        if (location == null)
        {
            await DisplayAlert("Error", "No se pudo obtener la ubicación.", "OK");
            return;
        }

        double userLat = location.Latitude;
        double userLon = location.Longitude;

        // 1. Obtiene las cumbres cercanas al usuario
        //var nearbySummits = await _summitService.GetNearbySummitsAsync(userLat, userLon, 0.009);~1km
        var nearbySummits = await _summitService.GetSummitListAsync();

        // 2. Filtra las cumbres que están dentro de un radio de 50 metros
        var nearest = nearbySummits
            .Select(s => new {
                Summit = s,
                Distance = Core.Helpers.Helpers.DistanceInMeters(userLat, userLon, s.Latitude, s.Longitude)
            })
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        if (nearest != null && nearest.Distance < 50)
        {
            // 3. Obtener el userId
            
            string userId = await SecureStorage.GetAsync("FirebaseUid"); ;
            if (string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Error", "No se pudo identificar al usuario.", "OK");
                return;
            }

            // 4. Valida si ya existe una ascensión hoy
            bool exists = await _ascentService.AscentExistsTodayAsync(userId, nearest.Summit.Id, DateTime.Now);
            if (exists)
            {
                await DisplayAlert("Aviso", $"Ya has registrado una ascensión a {(nearest.Summit.Name).ToUpper()} hoy. Inténtalo mañana de nuevo", "OK");
                return;
            }

            // 5. Registra la ascensión
            var ascent = new Ascent
            {
                UserId = userId,
                SummitId = nearest.Summit.Id,             
                ValidationMethod = "GPS"
            };
            await _ascentService.RegisterAscentAsync(ascent);

            await DisplayAlert("Registro Completado", $"¡Ascensión registrada: {nearest.Summit.Name}!", "OK");

        }
        else
        {
            await DisplayAlert("Error", "No estás cerca de ninguna cumbre registrada.", "OK");
        }
    }

    private async void OnRegistrarQrNfcClicked(object sender, EventArgs e)
    {
        // Implementar la funcionalidad de escaneo de QR/NFC
        await DisplayAlert("Info", "Funcionalidad de registro por QR/NFC pendiente de implementar.", "OK");
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}