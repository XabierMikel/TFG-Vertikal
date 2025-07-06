
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;

namespace Vertikal.Views;

public partial class MainPage : ContentPage
{
    private readonly IFirebaseSummitService _summitService;
    private readonly IFirebaseAuthService _authService;
    private readonly IFirebaseAscentService _ascentService;
    private readonly IFirebaseUsersServices _userSercvice;


    public MainPage(IFirebaseSummitService summitService, IFirebaseAuthService authService, IFirebaseAscentService ascentService, IFirebaseUsersServices usersServices)
    {
        InitializeComponent();
        _summitService = summitService;
        _authService = authService;
        _ascentService = ascentService;
        _userSercvice = usersServices;
        
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var userId = await SecureStorage.GetAsync("FirebaseUid");
        var user = await _userSercvice.GetUserByUidAsync(userId);
        var userName = user.Name ?? "Usuario";
        var ascents = await _ascentService.GetAscentsByUserIdAsync(userId);
        var allSummits = await _summitService.GetSummitListAsync();

        // Calcular el desnivel acumulado sumando la altitud de cada ascensión
        double desnivelTotal = 0;
        if (ascents != null && allSummits != null)
        {
            foreach (var ascent in ascents)
            {
                var summit = allSummits.FirstOrDefault(s => s.Id == ascent.SummitId);
                if (summit != null)
                    desnivelTotal += summit.Altitud;
            }
        }

        YearLabel.Text = DateTime.Now.Year.ToString(); 
        UserNameLabel.Text = userName;
        UserStatsLabel.Text = $"Ascensiones registradas: {ascents?.Count ?? 0}\nDesnivel total: {desnivelTotal} m";
       
    }

    private async void OnRegistrarAscensionClicked(object sender, EventArgs e)
    {
      
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1)));

        if (location == null)
        {
            await DisplayAlert("Error", "No se pudo obtener la ubicación.", "OK");
            return;
        }

        double userLat = location.Latitude;
        double userLon = location.Longitude;

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

    private async void OnCerrarSesionClicked(object sender, EventArgs e)
    {
        // Elimina las datos guardadas
        await _authService.LogoutAsync();
        // Redirige a la pantalla de registro/inicio de sesión
        await Shell.Current.GoToAsync("//RegisterPage"); 
    }


    private async void OnVerSummitsClicked(object sender, EventArgs e)
    {
        // Navegar a la pantalla de cumbres
        await Shell.Current.GoToAsync("//SummitsPage");
    }

    private async void OnVerHistorialClicked(object sender, EventArgs e)
    {
        // Navegar a la pantalla de historial 
        await Shell.Current.GoToAsync("//HistorialPage");
    }



    private async void OnPerfilUsuarioClicked(object sender, EventArgs e)
    {
        // Navegar a la pantalla de perfil de usuario
        await Shell.Current.GoToAsync("//UserProfilePage");
    }
}

