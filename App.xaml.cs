using Vertikal.Core.Interfaces;


namespace Vertikal
{
    public partial class App : Application
    {
        private readonly IFirebaseAuthService _authService;

        public App(IFirebaseAuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            MainPage = new AppShell(authService);
        }
    }
}
