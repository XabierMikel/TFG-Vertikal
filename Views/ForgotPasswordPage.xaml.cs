using Vertikal.Core.Interfaces;
namespace Vertikal.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly IFirebaseAuthService _authService;
    private readonly IApiClient _apiClient;

    public ForgotPasswordPage(IApiClient apiClient, IFirebaseAuthService authService)
	{
		InitializeComponent();
         _apiClient = apiClient;
        _authService = authService;
    }

    private async void OnResetPasswordClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;

        if (string.IsNullOrEmpty(email))
        {
            await DisplayAlert("Error", "Por favor, ingresa tu correo electr�nico.", "OK");
            return;
        }

        try
        {
            await _authService.ResetPasswordAsync(email);
            await DisplayAlert("�xito", "Se ha enviado un correo para restablecer tu contrase�a.", "OK");
            await Shell.Current.GoToAsync(".."); // Volver a la p�gina anterior
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo restablecer la contrase�a: {ex.Message}", "OK");
        }
    }

}