using Vertikal.Core.Models;
using Vertikal.Core.Interfaces;

namespace Vertikal.Views;

public partial class UserProfilePage : ContentPage
{
    private readonly IFirebaseUsersServices _usersService;
    private Users _user;
    private string _uid;
    private string _originalName = "";


    public UserProfilePage(IFirebaseUsersServices usersService)
    {
        InitializeComponent();
        _usersService = usersService;
       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUser();
    }

    private async void LoadUser()
    {
        _uid = await SecureStorage.GetAsync("FirebaseUid");
        if (string.IsNullOrEmpty(_uid))
        {
            await DisplayAlert("Error", "No se pudo identificar al usuario.", "OK");
            return;
        }
        _user = await _usersService.GetUserByUidAsync(_uid);
        if (_user != null)
        {
            EmailLabel.Text = _user.Email;   
            if (DateTime.TryParse(_user.RegisterDate, out var fecha))
                RegisterDateLabel.Text = fecha.ToString("dd/MM/yyyy");
            else
                RegisterDateLabel.Text = _user.RegisterDate;

            NameEntry.Text = _user.Name;
            _originalName = _user.Name ?? "";
            SaveButton.IsVisible = false;
            BackButton.IsVisible = true;
     
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var newName = NameEntry.Text?.Trim();
        if (string.IsNullOrEmpty(newName))
        {
            await DisplayAlert("Error", "El nombre no puede estar vacío.", "OK");
            return;
        }

        try
        {
            await _usersService.UpdateUserNameAsync(_uid, newName);
            await DisplayAlert("Éxito", "Nombre actualizado correctamente.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo actualizar el nombre: {ex.Message}", "OK");
        }

        await Shell.Current.GoToAsync("//MainPage");
    }

    private void OnNameChanged(object sender, TextChangedEventArgs e)
    {
        var newName = e.NewTextValue?.Trim() ?? "";
        bool changed = !string.Equals(newName, _originalName, StringComparison.Ordinal);
        SaveButton.IsVisible = changed;
        BackButton.IsVisible = !changed;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}