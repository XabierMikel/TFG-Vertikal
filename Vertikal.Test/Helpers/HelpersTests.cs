using System.Text.Json;
using Vertikal.Core.Helpers;
using Xunit;

namespace Vertikal.Test.Helpers
{
    public class HelpersTests
    {
        [Fact]
        public void GetDoubleFromField_ReturnsDouble_WhenDoubleValue()
        {
            var json = """
            {
                "Altitud": { "doubleValue": "123,45" }
            }
            """;
            var doc = JsonDocument.Parse(json);
            var fields = doc.RootElement;

            var result = Core.Helpers.Helpers.GetDoubleFromField(fields, "Altitud");

            Assert.Equal(123.45, result);
        }

        [Fact]
        public void GetDoubleFromField_ReturnsDouble_WhenStringValue()
        {
            var json = """
            {
                "Altitud": { "stringValue": "99,9" }
            }
            """;
            var doc = JsonDocument.Parse(json);
            var fields = doc.RootElement;

            var result = Core.Helpers.Helpers.GetDoubleFromField(fields, "Altitud");

            Assert.Equal(99.9, result);
        }

        [Fact]
        public void GetDoubleFromField_Throws_WhenMissing()
        {
            var json = "{}";
            var doc = JsonDocument.Parse(json);
            var fields = doc.RootElement;

            Assert.Throws<Exception>(() => Core.Helpers.Helpers.GetDoubleFromField(fields, "Altitud"));
        }

        [Fact]
        public void DistanceInMeters_ReturnsZero_ForSamePoint()
        {
            double lat = 43.0, lon = -2.0;
            double result = Core.Helpers.Helpers.DistanceInMeters(lat, lon, lat, lon);
            Assert.True(result < 0.01); // Debe ser prácticamente cero
        }

        [Fact]
        public void DistanceInMeters_ReturnsCorrectDistance_BilbaoToMadrid()
        {
            // Bilbao: 43.2630, -2.9350
            // Madrid: 40.4168, -3.7038
            double result = Core.Helpers.Helpers.DistanceInMeters(43.2630, -2.9350, 40.4168, -3.7038);
            // La distancia real ronda los 322 km
            Assert.InRange(result, 320_000, 330_000);
        }

        [Fact]
        public void DistanceInMeters_ReturnsCorrectDistance_OneDegreeLatitude()
        {
            // Un grado de latitud son ~111 km
            double result = Core.Helpers.Helpers.DistanceInMeters(0, 0, 1, 0);
            Assert.InRange(result, 110_000, 112_000);
        }

        [Fact]
        public void DistanceInMeters_ReturnsApprox50Meters()
        {
            // Un grado de latitud ? 111_139 metros, así que 50 metros ? 0.000449 grados
            double lat1 = 40.0, lon1 = -3.0;
            double lat2 = lat1 + (50.0 / 111_139.0); // ? 50 metros al norte

            double result = Core.Helpers.Helpers.DistanceInMeters(lat1, lon1, lat2, lon1);

            Assert.InRange(result, 49.0, 51.0);
        }
    }
}