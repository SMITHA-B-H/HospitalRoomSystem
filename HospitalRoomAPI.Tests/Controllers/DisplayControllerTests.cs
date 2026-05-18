using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class DisplayControllerTests
    {
        private readonly Mock<IDisplayService> _mockService;
        private readonly DisplayController _controller;

        public DisplayControllerTests()
        {
            _mockService = new Mock<IDisplayService>();

            _controller = new DisplayController(
                _mockService.Object
            );
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsOk_WhenRoomExists()
        {
            var roomNumber = "101";

            var mockResponse = new RoomDisplay
            {
                roomNumber = "101",
                roomName = "ICU",
                beds = new List<BedDisplayDto>(),
                videos = new List<string>(),
                announcements = new List<AnnouncementDisplayDto>() // ? FIXED
            };

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(mockResponse);

            var result = await _controller.GetRoomDisplay(roomNumber);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<RoomDisplay>(okResult.Value);

            Assert.Equal("101", value.roomNumber);
            Assert.Equal("ICU", value.roomName);
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsNotFound_WhenRoomDoesNotExist()
        {
            var roomNumber = "999";

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync((RoomDisplay?)null);

            var result = await _controller.GetRoomDisplay(roomNumber);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Room not found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetRoomDisplay_CallsService_Once()
        {
            var roomNumber = "102";

            var mockResponse = new RoomDisplay
            {
                roomNumber = "102",
                roomName = "Ward",
                beds = new List<BedDisplayDto>(),
                videos = new List<string>(),
                announcements = new List<AnnouncementDisplayDto>() // ? FIXED
            };

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(mockResponse);

            await _controller.GetRoomDisplay(roomNumber);

            _mockService.Verify(s => s.BuildRoomDisplay(roomNumber), Times.Once);
        }

        [Fact]
        public async Task GetRoomDisplay_ReturnsCorrectRoomData()
        {
            var roomNumber = "103";

            var expectedData = new RoomDisplay
            {
                roomNumber = "103",
                roomName = "General Ward",
                beds = new List<BedDisplayDto>(),
                videos = new List<string>(),
                announcements = new List<AnnouncementDisplayDto>() // ? FIXED
            };

            _mockService
                .Setup(s => s.BuildRoomDisplay(roomNumber))
                .ReturnsAsync(expectedData);

            var result = await _controller.GetRoomDisplay(roomNumber);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<RoomDisplay>(okResult.Value);

            Assert.Equal("103", value.roomNumber);
            Assert.Equal("General Ward", value.roomName);
        }
    }
}