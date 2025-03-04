using Marelli.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace Marelli.Test.Middlewares
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock;
        private readonly ExceptionMiddleware _middleware;

        public ExceptionMiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            _middleware = new ExceptionMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task InvokeAsync_Should_SetStatusCodeToInternalServerError_WhenUnhandledExceptionOccurs()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new Exception("Test exception"));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
            Assert.Contains("application/json", context.Response.ContentType);
        }


        [Fact]
        public async Task InvokeAsync_Should_SetStatusCodeToBadRequest_WhenArgumentExceptionOccurs()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new ArgumentException("Invalid argument"));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
            Assert.Contains("application/json", context.Response.ContentType);
        }

        [Fact]
        public async Task InvokeAsync_Should_SetStatusCodeToUnauthorized_WhenUnauthorizedAccessExceptionOccurs()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            _nextMock.Setup(next => next(It.IsAny<HttpContext>())).Throws(new UnauthorizedAccessException("Unauthorized"));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
            Assert.Contains("application/json", context.Response.ContentType);
        }
    }
}
