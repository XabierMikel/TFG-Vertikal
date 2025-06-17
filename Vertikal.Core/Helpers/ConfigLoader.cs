using Newtonsoft.Json;
using Vertikal.Core.Models;

namespace Vertikal.Core.Helpers;

public static class ConfigLoader
{
    public static FirebaseConfig ParseFirebaseConfig(string json)
    {
        var root = JsonConvert.DeserializeObject<Dictionary<string, FirebaseConfig>>(json);
        if (root != null && root.TryGetValue("Firebase", out var config))
        {
            return config;
        }

        throw new InvalidOperationException("No se encontró la configuración 'Firebase' en el JSON.");
    }
}