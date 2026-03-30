using Xunit;
using Moq;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HospitalRoomAPI.Tests.Controllers
{
    public class AnnouncementsControllerTests
    {
        [Fact]
        public async System.Threading.Tasks.Task GetAll_ReturnsOk()
        {
            var svc = new Mock<IAnnouncementService>();
            svc.Setup(s => s.GetAllAsync()).ReturnsAsync(new Models.Common.ApiResponse<object> { Success = true, Data = new System.Collections.Generic.List<object>() });

            var controller = new AnnouncementsController(svc.Object);
            var result = await controller.GetAll();
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
