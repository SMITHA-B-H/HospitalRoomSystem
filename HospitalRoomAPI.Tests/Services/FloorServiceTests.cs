using Xunit;
using Moq;
using FluentAssertions;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalRoomAPI.Tests.Services
{
    public class FloorServiceTests
    {
        private readonly Mock<IFloorRepository> _repoMock;
        private readonly Mock<IDisplayService> _displayMock;
        private readonly FloorService _service;

        public FloorServiceTests()
        {
            _repoMock = new Mock<IFloorRepository>();
            _displayMock = new Mock<IDisplayService>();

            _service = new FloorService(_repoMock.Object, _displayMock.Object);
        }

        [Fact]
        public async Task GetFloors_Should_Return_List()
        {
            _repoMock.Setup(r => r.GetFloorsAsync(1))
                .ReturnsAsync(new List<Floor> { new Floor { FloorName = "1st" } });

            var result = await _service.GetFloorsAsync(1);

            result.Success.Should().BeTrue();
            result.Data!.Count.Should().Be(1);
        }

       

        [Fact]
        public async Task UpdateFloor_Should_Update_And_Call_Display()
        {
            var floor = new Floor
            {
                Id = 1,
                Users = new List<User>
                {
                    new User { Role = "FloorAdmin" }
                }
            };

            _repoMock.Setup(r => r.GetFloorByIdAsync(1)).ReturnsAsync(floor);
            _repoMock.Setup(r => r.GetRoomNumbersByFloorIdAsync(1))
                .ReturnsAsync(new List<string> { "101" });

            var dto = new UpdateFloorDto { FloorName = "Updated" };

            var result = await _service.UpdateFloorAsync(1, dto);

            result.Success.Should().BeTrue();

            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

        [Fact]
        public async Task DeleteFloor_Should_Remove_And_Call_Display()
        {
            var floor = new Floor { Id = 1 };

            _repoMock.Setup(r => r.GetFloorByIdAsync(1)).ReturnsAsync(floor);
            _repoMock.Setup(r => r.GetRoomNumbersByFloorIdAsync(1))
                .ReturnsAsync(new List<string> { "101" });

            var result = await _service.DeleteFloorAsync(1);

            result.Success.Should().BeTrue();

            _repoMock.Verify(r => r.DeleteFloorAsync(floor), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

      

        [Fact]
        public async Task DeleteFloor_Should_Fail_When_Rooms_Exist()
        {
            var floor = new Floor
            {
                Id = 1,
                Rooms = new List<Room>
        {
            new Room { Id = 1, RoomNumber = "101" }
        }
            };

            _repoMock
                .Setup(r => r.GetFloorByIdAsync(1))
                .ReturnsAsync(floor);

            var result = await _service.DeleteFloorAsync(1);

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be("Floor cannot be deleted because rooms exist.");

            _repoMock.Verify(
                r => r.DeleteFloorAsync(It.IsAny<Floor>()),
                Times.Never);
        }


        [Fact]
        public async Task AddFloor_Should_Fail_When_Duplicate_Floor_Exists()
        {
            var dto = new CreateFloorDto
            {
                FloorName = "1st Floor"
            };

            _repoMock.Setup(r => r.GetFloorsAsync(1))
                .ReturnsAsync(new List<Floor>
                {
            new Floor { FloorName = "1st Floor" }
                });

            var result = await _service.AddFloorAsync(dto, 1);

            result.Success.Should().BeFalse();

            result.Message.Should()
                .Be("Floor already exists.");

            _repoMock.Verify(
                r => r.AddFloorAsync(It.IsAny<Floor>()),
                Times.Never);
        }

        [Fact]
        public async Task AddFloor_Should_Save_And_Return_Floor()
        {
            // Arrange
            var dto = new CreateFloorDto
            {
                FloorName = "2nd"
            };

            _repoMock.Setup(r => r.GetFloorsAsync(1))
                .ReturnsAsync(new List<Floor>());

            _repoMock.Setup(r => r.GetRoomNumbersByFloorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.AddFloorAsync(dto, 1);

            // Assert
            result.Success.Should().BeTrue();

            _repoMock.Verify(r => r.AddFloorAsync(It.IsAny<Floor>()), Times.Once);

            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

    }
}