using Firebase.Auth;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vertikal.Core.Interfaces;
using Vertikal.Core.Services;
using Xunit;

namespace Vertikal.Test.Services
{
    public class FirebaseAuthServiceTests
    {
        private readonly Mock<IAuthClient> _mockAuthClient;
        private readonly Mock<IApiClient> _mockApiClient;
        private readonly Mock<ISecureStorageService> _mockSecureStorage;
        private readonly FirebaseAuthService _service;
        private const string ApiKey = "fake-api-key";

        public FirebaseAuthServiceTests()
        {
            _mockAuthClient = new Mock<IAuthClient>();
            _mockApiClient = new Mock<IApiClient>();
            _mockSecureStorage = new Mock<ISecureStorageService>();
            _service = new FirebaseAuthService(_mockApiClient.Object, _mockAuthClient.Object, _mockSecureStorage.Object, ApiKey);
        }

        [Fact]
        public async Task LoginOrRegisterUserAsync_LoginSuccess_SavesTokensAndReturnsUid()
        {
            var mockResult = new Mock<IAuthResult>();
            mockResult.Setup(x => x.GetIdTokenAsync()).ReturnsAsync("idToken");
            mockResult.SetupGet(x => x.RefreshToken).Returns("refreshToken");
            mockResult.SetupGet(x => x.Uid).Returns("uid123");

            _mockAuthClient.Setup(x => x.SignInWithEmailAndPasswordAsync("test@email.com", "pass"))
                .ReturnsAsync(mockResult.Object);

            var uid = await _service.LoginOrRegisterUserAsync("test@email.com", "pass");

            Assert.Equal("uid123", uid);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseToken", "idToken"), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseRefreshToken", "refreshToken"), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseUid", "uid123"), Times.Once);
        }

        [Fact]
        public async Task LoginOrRegisterUserAsync_UnknownEmail_RegistersUser()
        {
            var mockResult = new Mock<IAuthResult>();
            mockResult.Setup(x => x.GetIdTokenAsync()).ReturnsAsync("idToken2");
            mockResult.SetupGet(x => x.RefreshToken).Returns("refreshToken2");
            mockResult.SetupGet(x => x.Uid).Returns("uid456");

            _mockAuthClient.Setup(x => x.SignInWithEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new FirebaseAuthException("not found",AuthErrorReason.UnknownEmailAddress));
            _mockAuthClient.Setup(x => x.CreateUserWithEmailAndPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(mockResult.Object);

            var uid = await _service.LoginOrRegisterUserAsync("nuevo@email.com", "pass");

            Assert.Equal("uid456", uid);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseToken", "idToken2"), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseRefreshToken", "refreshToken2"), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseUid", "uid456"), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordAsync_CallsAuthClient()
        {
            await _service.ResetPasswordAsync("reset@email.com");
            _mockAuthClient.Verify(x => x.ResetEmailPasswordAsync("reset@email.com"), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ClearsTokens()
        {
            await _service.LogoutAsync();
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseToken", ""), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseRefreshToken", ""), Times.Once);
            _mockSecureStorage.Verify(x => x.SetAsync("FirebaseUid", ""), Times.Once);
        }

        [Fact]
        public async Task GetValidIdTokenAsync_Throws_WhenTokensAreMissing()
        {
            _mockSecureStorage.Setup(x => x.GetAsync("FirebaseToken")).ReturnsAsync((string)null);
            _mockSecureStorage.Setup(x => x.GetAsync("FirebaseRefreshToken")).ReturnsAsync((string)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetValidIdTokenAsync());
            Assert.Contains("Usuario no autenticado", ex.Message);
        }


    }
}