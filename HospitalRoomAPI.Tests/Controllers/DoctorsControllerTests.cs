using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class DoctorsControllerTests
    {
        private DoctorsController CreateController(Mock<IDoctorService> svcMock, Mock<IWebHostEnvironment> envMock)
        {
            var controller = new DoctorsController(svcMock.Object, envMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim("HospitalId", "1"), new Claim(ClaimTypes.Role, "SuperAdmin")
            }, "Test"));

            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            return controller;
        }

        [Fact]
        public async System.Threading.Tasks.Task GetDoctors_ReturnsOk()
        {
            var svc = new Mock<IDoctorService>();
            svc.Setup(s => s.GetDoctorsAsync(1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true, Data = new System.Collections.Generic.List<object>() });
            var env = new Mock<IWebHostEnvironment>();

            var controller = CreateController(svc, env);

            var result = await controller.GetDoctors();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddDoctor_Forbid_WhenServiceFails()
        {
            var svc = new Mock<IDoctorService>();
            svc.Setup(s => s.AddDoctorAsync(It.IsAny<DoctorDto>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
               .ReturnsAsync(new Models.Common.ApiResponse<object> { Success = false });

            var env = new Mock<IWebHostEnvironment>();
            env.Setup(e => e.WebRootPath).Returns("wwwroot");

            var controller = CreateController(svc, env);

            var dto = new DoctorDto { Name = "Dr" };
            var result = await controller.AddDoctor(dto);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteDoctor_NotFound_WhenServiceFails()
        {
            var svc = new Mock<IDoctorService>();
            svc.Setup(s => s.DeleteDoctorAsync(1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = false });
            var env = new Mock<IWebHostEnvironment>();

            var controller = CreateController(svc, env);

            var result = await controller.DeleteDoctor(1);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
