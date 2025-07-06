using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Vertikal.Core.Helpers
{
    public static class Helpers
    {
        public static double GetDoubleFromField(JsonElement fields, string propertyName)
        {
            if (fields.TryGetProperty(propertyName, out var prop))
            {
                if (prop.TryGetProperty("stringValue", out var strVal) &&
                    double.TryParse(strVal.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result))
                    return result;

                if (prop.TryGetProperty("doubleValue", out var doubleVal))
                {
                    // ✅ Si es número, úsalo directamente
                    if (doubleVal.ValueKind == JsonValueKind.Number)
                        return doubleVal.GetDouble();

                    // En otros casos raros (número como string), parseamos
                    if (doubleVal.ValueKind == JsonValueKind.String &&
                        double.TryParse(doubleVal.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result))
                        return result;
                }

                if (prop.TryGetProperty("integerValue", out var intVal))
                {
                    if (intVal.ValueKind == JsonValueKind.Number)
                        return intVal.GetDouble();

                    if (intVal.ValueKind == JsonValueKind.String &&
                        double.TryParse(intVal.GetString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result))
                        return result;
                }
            }

            throw new Exception($"No se pudo obtener el valor double para '{propertyName}'.");
        }


        public  static double DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var rlat1 = Math.PI * lat1 / 180.0;
            var rlat2 = Math.PI * lat2 / 180.0;
            var theta = lon1 - lon2;
            var rtheta = Math.PI * theta / 180.0;
            var dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180.0 / Math.PI;
            dist = dist * 60 * 1.1515 * 1609.344; // metros
            return dist;
        }
    }
}
