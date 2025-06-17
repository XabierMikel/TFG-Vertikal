using Vertikal.Core.Interfaces;
using Vertikal.Views;

namespace Vertikal
{
    public partial class AppShell : Shell
    {
        private readonly IFirebaseAuthService _authService;

        public AppShell(IFirebaseAuthService authService)
        {
            InitializeComponent();
            _authService = authService;

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("ForgotPasswordPage", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("RegisterPage", typeof(RegisterPage));
            Routing.RegisterRoute("HistorialPage", typeof(HistorialPage));
            Routing.RegisterRoute("AscentRegisterPage", typeof(AscentRegisterPage));
            Routing.RegisterRoute("SummitsPage", typeof(SummitsPage));
            Routing.RegisterRoute("UserProfilePage", typeof(UserProfilePage));
          


        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var token = await _authService.GetValidIdTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    if (CurrentItem?.Route != "MainPage")
                        await GoToAsync("//MainPage");
                }
                else
                {
                    if (CurrentItem?.Route != "RegisterPage")
                        await GoToAsync("//RegisterPage");
                }
            }
            catch (Exception ex)
            {
                // Si ocurre un error, navega a la página de registro
                if (CurrentItem?.Route != "RegisterPage")
                        await GoToAsync("//RegisterPage");
                
             
            }
        }

    }


}
