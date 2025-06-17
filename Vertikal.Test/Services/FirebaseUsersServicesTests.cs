using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Models;
using Vertikal.Core.Services;
using Xunit;

namespace Vertikal.Test.Services
{
    public class FirebaseUsersServicesTests
    {
        private readonly Mock<IFirebaseAuthService> _mockAuthService;
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly FirebaseUsersServices _service;

        public FirebaseUsersServicesTests()
        {
            _mockAuthService = new Mock<IFirebaseAuthService>();
            _mockApiClient = new Mock<IApiClient>();
            _service = new FirebaseUsersServices(_mockAuthService.Object, _mockApiClient.Object);
        }

        [Fact]
        public async Task SaveUserToFirestoreAsync_Success()
        {
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _service.SaveUserToFirestoreAsync("uid123", "test@email.com");

            _mockApiClient.Verify(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()), Times.Once);
        }

        [Fact]
        public async Task GetUserByUidAsync_ReturnsUser()
        {
            var json = """
            {
              "fields": {
                "Email": { "stringValue": "test@email.com" },
                "Name": { "stringValue": "Test User" },
                "RegisterDate": { "stringValue": "2025-06-10T12:00:00Z" }
              }
            }
            """;
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var user = await _service.GetUserByUidAsync("uid123");

            Assert.NotNull(user);
            Assert.Equal("test@email.com", user.Email);
            Assert.Equal("Test User", user.Name);
            Assert.Equal("2025-06-10T12:00:00Z", user.RegisterDate);
        }

        [Fact]
        public async Task GetUserByUidAsync_ReturnsNull_OnNotFound()
        {
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var user = await _service.GetUserByUidAsync("uid123");
            Assert.Null(user);
        }

        [Fact]
        public async Task UpdateUserNameAsync_Success()
        {
            _mockAuthService.Setup(x => x.GetValidIdTokenAsync()).ReturnsAsync("token");
            _mockApiClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            await _service.UpdateUserNameAsync("uid123", "NuevoNombre");

            _mockApiClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
        }
    }
}