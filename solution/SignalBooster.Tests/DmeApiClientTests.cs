using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Synapse.SignalBoosterExample;
using Synapse.SignalBoosterExample.Services;
using Xunit;
using static Synapse.SignalBoosterExample.SignalBoosterEnums;

namespace SignalBooster.Tests
{
    public class DmeApiClientTests
    {
        private readonly Mock<ILogger<DmeApiClient>> _loggerMock;
        private readonly string _baseApiUrl = "https://alert-api.com";

        public DmeApiClientTests()
        {
            _loggerMock = new Mock<ILogger<DmeApiClient>>();
        }

        [Fact]
        public void Constructor_WithNullBaseUrl_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DmeApiClient(baseApiUrl: null!, _loggerMock.Object));
            Assert.Equal("baseApiUrl", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DmeApiClient(_baseApiUrl, logger: null!));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new DmeApiClient(_baseApiUrl, httpClient: null!, _loggerMock.Object));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        public void TransmitDmeData_WithSuccessResponse_ReturnsStatusCode200()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var jsonPayload = "{\"device\":\"CPAP\",\"ordering_provider\":\"Dr. Test\"}";

            // Act
            int statusCode = apiClient.TransmitDmeData(jsonPayload);

            // Assert
            Assert.Equal(200, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public async Task TransmitDmeDataAsync_WithSuccessResponse_ReturnsStatusCode200()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var jsonPayload = "{\"device\":\"CPAP\",\"ordering_provider\":\"Dr. Test\"}";

            // Act
            int statusCode = await apiClient.TransmitDmeDataAsync(jsonPayload);

            // Assert
            Assert.Equal(200, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public void TransmitDmeData_WithErrorResponse_ReturnsErrorStatusCode()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.NotFound);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var jsonPayload = "{\"device\":\"CPAP\",\"ordering_provider\":\"Dr. Test\"}";

            // Act
            int statusCode = apiClient.TransmitDmeData(jsonPayload);

            // Assert
            Assert.Equal(404, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public void TransmitDmeData_WithException_ReturnsInternalServerError()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Simulated network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var jsonPayload = "{\"device\":\"CPAP\",\"ordering_provider\":\"Dr. Test\"}";

            // Act
            int statusCode = apiClient.TransmitDmeData(jsonPayload);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public void TransmitDmeObject_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                apiClient.TransmitDmeObject(dmeData: null!));
            Assert.Equal("dmeData", exception.ParamName);
        }

        [Fact]
        public void TransmitDmeObject_WithValidObject_CallsTransmitDmeData()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var dmeObject = new DurableMedicalEquipment
            {
                DeviceType = DeviceType.CPAP,
                OrderingProvider = "Dr. Test",
                MaskType = CpapMaskType.FullFace,
                AddOn = AddOnOption.Humidifier,
                ApneaHypopneaIndex = 20
            };

            // Act
            int statusCode = apiClient.TransmitDmeObject(dmeObject);

            // Assert
            Assert.Equal(200, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public async Task TransmitDmeObjectAsync_WithNullObject_ThrowsArgumentNullException()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                apiClient.TransmitDmeObjectAsync(dmeData: null!));
            Assert.Equal("dmeData", exception.ParamName);
        }

        [Fact]
        public async Task TransmitDmeObjectAsync_WithValidObject_CallsTransmitDmeDataAsync()
        {
            // Arrange
            var handlerMock = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(handlerMock.Object);
            var apiClient = new DmeApiClient(_baseApiUrl, httpClient, _loggerMock.Object);
            var dmeObject = new DurableMedicalEquipment
            {
                DeviceType = DeviceType.OxygenTank,
                OrderingProvider = "Dr. Test",
                OxygenTankCapacity = 5.0,
                OxygenUsageContext = OxygenTankUsageContext.SleepAndExertion
            };

            // Act
            int statusCode = await apiClient.TransmitDmeObjectAsync(dmeObject);

            // Assert
            Assert.Equal(200, statusCode);
            VerifyHttpClientCall(handlerMock, $"{_baseApiUrl}/DrExtract", HttpMethod.Post, Times.Once());
        }

        [Fact]
        public void Dispose_DisposesHttpClient()
        {
            // This test is tricky since we can't easily check if HttpClient is disposed
            // But we can check that calling Dispose multiple times doesn't throw

            // Arrange
            var apiClient = new DmeApiClient(_baseApiUrl, _loggerMock.Object);

            // Act & Assert (no exception should be thrown)
            apiClient.Dispose();
            apiClient.Dispose(); // Second call should be safe
        }

        #region Helper Methods

        private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode
                });

            return handlerMock;
        }

        private void VerifyHttpClientCall(
            Mock<HttpMessageHandler> handlerMock,
            string expectedUrl,
            HttpMethod expectedMethod,
            Times times)
        {
            handlerMock
                .Protected()
                .Verify(
                    "SendAsync",
                    times,
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == expectedMethod &&
                        req.RequestUri != null &&
                        req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>());
        }

        #endregion
    }
}
