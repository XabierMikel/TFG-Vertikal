using Vertikal.Core.Interfaces;

namespace Vertikal.Views;

public partial class RegisterPage : ContentPage
{
    private readonly IFirebaseAuthService _authService;
    private readonly IFirebaseUsersServices _usersService;

    public RegisterPage(IFirebaseAuthService authService, IFirebaseUsersServices usersServices)
	{
		InitializeComponent();
        _authService = authService;
        _usersService = usersServices;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing(); 
        EmailEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Por favor, ingresa tu correo y contraseña.", "OK");
            return;
        }

        try
        {
            // Intentar iniciar sesión
            var uid = await _authService.LoginOrRegisterUserAsync(email, password);

            // Intenta obtener el usuario en Firestore
            var user = await _usersService.GetUserByUidAsync(uid);

            if (user == null)
            {
                // Si no existe, lo crea
                await _usersService.SaveUserToFirestoreAsync(uid, email);
            }


            // Navegar a la pantalla principal
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
         
             await DisplayAlert("Error", $"No se pudo completar la operación: {ex.Message}", "OK");
            
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        // Navegar a la página de restablecimiento de contraseña
        await Shell.Current.GoToAsync("ForgotPasswordPage");
    }

}