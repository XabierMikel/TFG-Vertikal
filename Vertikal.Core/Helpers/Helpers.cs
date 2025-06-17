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
                if (prop.TryGetProperty("stringValue", out var strVal) && double.TryParse(strVal.GetString(), out var result))
                    return result;
                if (prop.TryGetProperty("doubleValue", out var doubleVal) && double.TryParse(doubleVal.GetString(), out result))
                    return result;
                if (prop.TryGetProperty("integerValue", out var intVal) && double.TryParse(intVal.GetString(), out result))
                    return result;
            }
            throw new Exception($"No se pudo obtener el valor double para {propertyName}");
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
