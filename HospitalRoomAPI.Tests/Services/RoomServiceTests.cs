using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;

namespace HospitalRoomAPI.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly Mock<IRoomRepository> _repoMock;
        private readonly Mock<IDisplayService> _displayMock;
        private readonly RoomService _service;

        public RoomServiceTests()
        {
            _repoMock = new Mock<IRoomRepository>();
            _displayMock = new Mock<IDisplayService>();

            _service = new RoomService(_repoMock.Object, _displayMock.Object);
        }

        // ================= EXISTING TESTS =================

        [Fact]
        public async Task CreateRoom_Should_Create_Beds_And_Save()
        {
            var room = new Room
            {
                RoomNumber = "101",
                TotalBeds = 3
            };

            var result = await _service.CreateRoomAsync(room, "SuperAdmin", 1, null);

            result.Success.Should().BeTrue();
            result.Data!.Beds.Should().HaveCount(3);

            _repoMock.Verify(r => r.AddRoomAsync(It.IsAny<Room>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

        [Fact]
        public async Task CreateRoom_FloorAdmin_Should_Override_FloorId()
        {
            var room = new Room
            {
                FloorId = 999,
                TotalBeds = 2
            };

            var result = await _service.CreateRoomAsync(room, "FloorAdmin", 1, 5);

            result.Data!.FloorId.Should().Be(5);
        }

        [Fact]
        public async Task GetRooms_Should_Return_Room_List()
        {
            var rooms = new List<Room>
            {
                new Room { RoomNumber = "101" },
                new Room { RoomNumber = "102" }
            };

            _repoMock
                .Setup(r => r.GetRoomsAsync("SuperAdmin", 1, null))
                .ReturnsAsync(rooms);

            var result = await _service.GetRoomsAsync("SuperAdmin", 1, null);

            result.Success.Should().BeTrue();

            var data = result.Data as List<Room>;
            data.Should().NotBeNull();
            data!.Count.Should().Be(2);
        }

        [Fact]
        public async Task CreateRoom_With_Zero_Beds_Should_Create_Empty_List()
        {
            var room = new Room
            {
                RoomNumber = "102",
                TotalBeds = 0
            };

            var result = await _service.CreateRoomAsync(room, "SuperAdmin", 1, null);

            result.Data!.Beds.Should().BeEmpty();
        }

        [Fact]
        public async Task CreateRoom_Should_Call_DisplayService()
        {
            var room = new Room
            {
                RoomNumber = "200",
                TotalBeds = 1
            };

            await _service.CreateRoomAsync(room, "SuperAdmin", 1, null);

            _displayMock.Verify(d => d.PushRoomUpdate("200"), Times.Once);
        }

        // ================= NEW TESTS =================

        // 🔹 Assign Patient Success
        [Fact]
        public async Task AssignPatient_Should_Assign_And_Update_Bed()
        {
            var bed = new Bed
            {
                Id = 1,
                Room = new Room { RoomNumber = "101" }
            };

            _repoMock.Setup(r => r.GetBedByIdAsync(1)).ReturnsAsync(bed);

            var dto = new AssignPatientDto
            {
                BedId = 1,
                Name = "John",
                Age = 30,
                DoctorId = 2
            };

            var result = await _service.AssignPatientAsync(dto);

            result.Success.Should().BeTrue();
            bed.Status.Should().Be("Occupied");

            _repoMock.Verify(r => r.AddPatientAsync(It.IsAny<Patient>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

        // 🔹 Assign Patient Fail (Bed Not Found)
        [Fact]
        public async Task AssignPatient_Should_Fail_When_Bed_Not_Found()
        {
            _repoMock.Setup(r => r.GetBedByIdAsync(1)).ReturnsAsync((Bed?)null);

            var dto = new AssignPatientDto { BedId = 1 };

            var result = await _service.AssignPatientAsync(dto);

            result.Success.Should().BeFalse();
        }

        // 🔹 Discharge Patient Success
        [Fact]
        public async Task DischargePatient_Should_Clear_Patient_And_Update_Bed()
        {
            var bed = new Bed
            {
                Id = 1,
                Patient = new Patient { Id = 10 },
                Room = new Room { RoomNumber = "101" }
            };

            _repoMock.Setup(r => r.GetBedByIdAsync(1)).ReturnsAsync(bed);

            var result = await _service.DischargePatientAsync(1);

            result.Success.Should().BeTrue();
            bed.Status.Should().Be("Available");

            _repoMock.Verify(r => r.RemovePatientByIdAsync(10), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

        // 🔹 Discharge Fail
        [Fact]
        public async Task DischargePatient_Should_Fail_When_Invalid()
        {
            _repoMock.Setup(r => r.GetBedByIdAsync(1)).ReturnsAsync((Bed?)null);

            var result = await _service.DischargePatientAsync(1);

            result.Success.Should().BeFalse();
        }

        // 🔹 Delete Room
        [Fact]
        public async Task DeleteRoom_Should_Remove_And_Call_Display()
        {
            var room = new Room
            {
                Id = 1,
                RoomNumber = "101"
            };

            _repoMock.Setup(r => r.GetRoomByIdWithBedsAsync(1)).ReturnsAsync(room);

            var result = await _service.DeleteRoomAsync(1, "SuperAdmin", 1, null);

            result.Success.Should().BeTrue();

            _repoMock.Verify(r => r.RemoveRoomAsync(room), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        }

        // 🔹 Update Room
        [Fact]
        public async Task UpdateRoom_Should_Update_And_Call_Display()
        {
            var room = new Room
            {
                Id = 1,
                RoomNumber = "101"
            };

            var updated = new Room
            {
                RoomNumber = "102",
                RoomName = "ICU",
                RoomType = "Critical",
                TotalBeds = 5
            };

            _repoMock.Setup(r => r.GetRoomByIdWithBedsAsync(1)).ReturnsAsync(room);

            var result = await _service.UpdateRoomAsync(1, updated, "SuperAdmin", 1, null);

            result.Success.Should().BeTrue();
            room.RoomNumber.Should().Be("102");

            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _displayMock.Verify(d => d.PushRoomUpdate("102"), Times.Once);
        }

        // 🔹 Get Rooms By Floor
        [Fact]
        public async Task GetRoomsByFloor_Should_Return_List()
        {
            _repoMock
                .Setup(r => r.GetRoomsByFloorAsync(1))
                .ReturnsAsync(new List<object> { new { RoomNumber = "101" } });

            var result = await _service.GetRoomsByFloorAsync(1);

            result.Should().NotBeNull();
        }
    }
}