using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Tests.Services
{
    public class DisplayServiceTests
    {
        private readonly Mock<IDisplayRepository> _repoMock;
        private readonly Mock<IHubContext<RoomHub>> _hubMock;
        private readonly Mock<IHubClients> _clientsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;

        private readonly DisplayService _service;

        public DisplayServiceTests()
        {
            _repoMock = new Mock<IDisplayRepository>();
            _hubMock = new Mock<IHubContext<RoomHub>>();
            _clientsMock = new Mock<IHubClients>();
            _clientProxyMock = new Mock<IClientProxy>();

            _clientsMock.Setup(c => c.Group(It.IsAny<string>()))
                        .Returns(_clientProxyMock.Object);

            _hubMock.Setup(h => h.Clients)
                    .Returns(_clientsMock.Object);

            _service = new DisplayService(_repoMock.Object, _hubMock.Object);
        }

        [Fact]
        public async Task BuildRoomDisplay_Should_Return_Data()
        {
            var room = new Room
            {
                Id = 1,
                RoomNumber = "101",
                RoomName = "ICU",
                RoomType = "Critical",
                FloorId = 1,
                Floor = new Floor
                {
                    Id = 1,
                    HospitalId = 1
                },
                Beds = new List<Bed>()
            };

            _repoMock.Setup(r => r.GetRoomWithDetailsAsync("101"))
                     .ReturnsAsync(room);

            var result = await _service.BuildRoomDisplay("101");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task BuildRoomDisplay_Should_Return_Null_When_NotFound()
        {
            _repoMock.Setup(r => r.GetRoomWithDetailsAsync("999"))
                     .ReturnsAsync((Room?)null);

            var result = await _service.BuildRoomDisplay("999");

            Assert.Null(result);
        }

        [Fact]
        public async Task PushRoomUpdate_Should_Call_SignalR()
        {
            var room = new Room
            {
                Id = 1,
                RoomNumber = "200",
                RoomName = "ICU",
                RoomType = "Critical",
                FloorId = 1,
                Floor = new Floor
                {
                    Id = 1,
                    HospitalId = 1
                },
                Beds = new List<Bed>()
            };

            _repoMock.Setup(r => r.GetRoomWithDetailsAsync("200"))
                     .ReturnsAsync(room);

            await _service.PushRoomUpdate("200");

            _clientProxyMock.Verify(
                x => x.SendCoreAsync(
                    "RoomUpdated",
                    It.IsAny<object[]>(),
                    default),
                Times.Once);
        }
    }
}