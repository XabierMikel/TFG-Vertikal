using Vertikal.Core.Helpers;
using Vertikal.Core.Models;

namespace Vertikal.Helpers
{
    public static class ConfigManager
    {
        public static FirebaseConfig LoadFirebaseConfigFromFile()
        {
            using var stream = FileSystem.OpenAppPackageFileAsync("appSettings.json").Result;
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            return ConfigLoader.ParseFirebaseConfig(json);
        }
    }
}
