using Xunit;
using Moq;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class FloorsControllerTests
    {
        private FloorsController CreateController(Mock<IFloorService> svc)
        {
            var controller = new FloorsController(svc.Object);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("HospitalId", "1") }, "Test"));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
            return controller;
        }

        [Fact]
        public async System.Threading.Tasks.Task GetFloors_ReturnsOk()
        {
            var svc = new Mock<IFloorService>();
            svc.Setup(s => s.GetFloorsAsync(1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true, Data = new System.Collections.Generic.List<object>() });

            var controller = CreateController(svc);
            var result = await controller.GetFloors();
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task AddFloor_ReturnsOk()
        {
            var svc = new Mock<IFloorService>();
            svc.Setup(s => s.AddFloorAsync(It.IsAny<CreateFloorDto>(), 1)).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true });

            var controller = CreateController(svc);
            var dto = new CreateFloorDto { FloorNumber = "1" };

            var result = await controller.AddFloor(dto);
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
