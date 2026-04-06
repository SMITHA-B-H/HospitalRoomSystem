using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using System.Threading.Tasks;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class DisplayControllerTests
    {
        private readonly Mock<IDisplayService> _mockService;
        private readonly DisplayController _controller;

        public DisplayControllerTests()
        {
            _mockService = new Mock<IDisplayService>();
            _controller = new DisplayController(_mockService.Object);
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsOk_WhenRoomExists()
        {
            // Arrange
            var roomNumber = "101";

            var mockResponse = new
            {
                RoomNumber = "101",
                RoomName = "ICU",
                TotalBeds = 5
            };

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.GetRoomDisplay(roomNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mockResponse, okResult.Value);
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsNotFound_WhenRoomDoesNotExist()
        {
            // Arrange
            var roomNumber = "999";

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync((object?)null);

            // Act
            var result = await _controller.GetRoomDisplay(roomNumber);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Room not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetRoomDisplay_CallsService_Once()
        {
            // Arrange
            var roomNumber = "102";

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(new { RoomNumber = roomNumber });

            // Act
            await _controller.GetRoomDisplay(roomNumber);

            // Assert
            _mockService.Verify(s => s.BuildRoomDisplay(roomNumber), Times.Once);
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsCorrectRoomData()
        {
            // Arrange
            var roomNumber = "103";

            var expectedData = new
            {
                RoomNumber = "103",
                RoomName = "General Ward",
                TotalBeds = 10
            };

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(expectedData);

            // Act
            var result = await _controller.GetRoomDisplay(roomNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            dynamic value = okResult.Value;

            Assert.Equal("103", value.RoomNumber);
            Assert.Equal("General Ward", value.RoomName);
            Assert.Equal(10, value.TotalBeds);
        }
    }
}