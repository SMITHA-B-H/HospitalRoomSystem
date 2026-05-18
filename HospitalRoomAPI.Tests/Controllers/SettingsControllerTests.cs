using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.Hubs;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class SettingsControllerTests
    {
        private readonly Mock<ISettingsService> _serviceMock = new();

        private readonly Mock<IHubContext<RoomHub>> _hubMock = new();

        private SettingsController GetController()
        {
            var clientProxyMock = new Mock<IClientProxy>();

            var hubClientsMock = new Mock<IHubClients>();

            hubClientsMock
                .Setup(c => c.All)
                .Returns(clientProxyMock.Object);

            _hubMock
                .Setup(h => h.Clients)
                .Returns(hubClientsMock.Object);

            var controller = new SettingsController(
                _serviceMock.Object,
                _hubMock.Object
            );

            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
            new Claim("HospitalId", "1")
                }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            return controller;
        }

        // ================= GET DISPLAY =================

        [Fact]
        public async Task GetDisplaySettings_ReturnsOk()
        {
            _serviceMock
                .Setup(s => s.GetDisplaySettings(1))
                .ReturnsAsync(new ApiResponse<object>
                {
                    Success = true
                });

            var controller = GetController();

            var result = await controller.GetDisplaySettings(1);

            var ok = Assert.IsType<OkObjectResult>(result);

            var data = Assert.IsType<ApiResponse<object>>(ok.Value);

            Assert.True(data.Success);
        }

        // ================= SAVE SETTINGS =================

        [Fact]
        public async Task SaveSettings_ReturnsOk()
        {
            var dto = new SaveSettingsDto();

            _serviceMock
                .Setup(s => s.SaveSettings(dto, 1))
                .ReturnsAsync(new ApiResponse<object>
                {
                    Success = true
                });

            var controller = GetController();

            var result = await controller.SaveSettings(dto);

            var ok = Assert.IsType<OkObjectResult>(result);

            var data = Assert.IsType<ApiResponse<object>>(ok.Value);

            Assert.True(data.Success);
        }

        // ================= UPLOAD LOGO =================

        [Fact]
        public async Task UploadLogo_ReturnsOk()
        {
            var fileMock = new Mock<IFormFile>();

            _serviceMock
                .Setup(s => s.UploadLogo(fileMock.Object, 1))
                .ReturnsAsync(new ApiResponse<string>
                {
                    Success = true,
                    Data = "logo.png"
                });

            var controller = GetController();

            var result = await controller.UploadLogo(fileMock.Object);

            var ok = Assert.IsType<OkObjectResult>(result);

            var data = Assert.IsType<ApiResponse<string>>(ok.Value);

            Assert.True(data.Success);

            Assert.Equal("logo.png", data.Data);
        }

        // ================= UPLOAD VIDEO =================

        [Fact]
        public async Task UploadVideo_ReturnsOk()
        {
            var dto = new UploadVideoDto();

            _serviceMock
                .Setup(s => s.UploadVideo(dto, 1))
                .ReturnsAsync(new ApiResponse<string>
                {
                    Success = true,
                    Data = "video.mp4"
                });

            var controller = GetController();

            var result = await controller.UploadVideo(dto);

            var ok = Assert.IsType<OkObjectResult>(result);

            var data = Assert.IsType<ApiResponse<string>>(ok.Value);

            Assert.True(data.Success);

            Assert.Equal("video.mp4", data.Data);
        }

        // ================= DELETE VIDEO =================

        [Fact]
        public async Task DeleteVideo_ReturnsOk()
        {
            string path = "/videos/test.mp4";

            _serviceMock
                .Setup(s => s.DeleteVideo(path, 1))
                .ReturnsAsync(new ApiResponse<object>
                {
                    Success = true
                });

            var controller = GetController();

            var result = await controller.DeleteVideo(path);

            var ok = Assert.IsType<OkObjectResult>(result);

            var data = Assert.IsType<ApiResponse<object>>(ok.Value);

            Assert.True(data.Success);
        }
    }
}