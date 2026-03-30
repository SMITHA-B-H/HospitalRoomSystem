using Xunit;
using Moq;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class SettingsControllerTests
    {
        private SettingsController CreateController(Mock<ISettingsService> svc)
        {
            var controller = new SettingsController(svc.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("HospitalId", "1") }, "Test"));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            return controller;
        }

        [Fact]
        public async System.Threading.Tasks.Task GetDisplaySettings_ReturnsOk()
        {
            var svc = new Mock<ISettingsService>();
            svc.Setup(s => s.GetDisplaySettings(1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true, Data = new { } });

            var controller = CreateController(svc);
            var result = await controller.GetDisplaySettings(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task SaveSettings_ReturnsOk()
        {
            var svc = new Mock<ISettingsService>();
            svc.Setup(s => s.SaveSettings(It.IsAny<SaveSettingsDto>(), 1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true });

            var controller = CreateController(svc);
            var dto = new SaveSettingsDto { AdsVolume = 50 };
            var result = await controller.SaveSettings(dto);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
