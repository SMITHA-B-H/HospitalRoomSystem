using Xunit;
using Moq;
using FluentAssertions;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.Hubs;
using HospitalRoomAPI.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;
using System.Collections.Generic;

namespace HospitalRoomAPI.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _repoMock;

        private readonly Mock<IDisplayService>
            _displayMock;

        private readonly Mock<IConfiguration>
            _configMock;

        private readonly Mock<IHubContext<RoomHub>>
            _hubMock;

        private readonly Mock<IHubClients>
            _clientsMock;

        private readonly Mock<IClientProxy>
            _clientProxyMock;

        private readonly AppDbContext _context;

        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            // =====================================
            // MOCKS
            // =====================================

            _repoMock =
                new Mock<IDoctorRepository>();

            _displayMock =
                new Mock<IDisplayService>();

            _configMock =
                new Mock<IConfiguration>();

            _hubMock =
                new Mock<IHubContext<RoomHub>>();

            _clientsMock =
                new Mock<IHubClients>();

            _clientProxyMock =
                new Mock<IClientProxy>();

            // =====================================
            // SIGNALR
            // =====================================

            _clientsMock.Setup(x => x.All)
                .Returns(_clientProxyMock.Object);

            _hubMock.Setup(x => x.Clients)
                .Returns(_clientsMock.Object);

            // =====================================
            // CONFIG
            // =====================================

            _configMock.Setup(x => x["StoragePath"])
                .Returns("C:\\Temp");

            // =====================================
            // IN MEMORY DB
            // =====================================

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    databaseName:
                    Guid.NewGuid().ToString()
                )
                .Options;

            _context =
                new AppDbContext(options);

            // =====================================
            // SERVICE
            // =====================================

            _service = new DoctorService(
                _repoMock.Object,
                _displayMock.Object,
                _configMock.Object,
                _hubMock.Object,
                _context
            );
        }

        // =====================================
        // GET DOCTORS
        // =====================================

        [Fact]
        public async Task
        GetDoctorsAsync_Should_Return_Doctors()
        {
            // Arrange

            var doctors =
                new List<Doctor>
                {
                    new Doctor
                    {
                        Id = 1,
                        Name = "Dr. Surendra"
                    }
                };

            _repoMock.Setup(x =>
                x.GetDoctorsAsync(1))
                .ReturnsAsync(doctors);

            // Act

            var result =
                await _service
                .GetDoctorsAsync(1);

            // Assert

            result.Success
                .Should().BeTrue();

            result.Data
                .Should().HaveCount(1);
        }

        // =====================================
        // ADD DOCTOR
        // =====================================

        [Fact]
        public async Task
        AddDoctorAsync_Should_Add_Doctor()
        {
            // Arrange

            var dto = new DoctorDto
            {
                EmployeeId = "EMP001",
                Name = "Dr. Test",
                Department = "Cardiology",
                Role = "Doctor"
            };

            // Act

            var result =
                await _service
                .AddDoctorAsync(
                    dto,
                    1,
                    "SuperAdmin"
                );

            // Assert

            result.Success
                .Should().BeTrue();

            result.Data
                .Should().NotBeNull();

            result.Data.Name
                .Should().Be("Dr. Test");

            _repoMock.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>()),
                Times.Once
            );
        }

        // =====================================
        // UPDATE DOCTOR
        // =====================================

        [Fact]
        public async Task
        UpdateDoctorAsync_Should_Update_Doctor()
        {
            // Arrange

            var doctor =
                new Doctor
                {
                    Id = 1,
                    Name = "Old Name",
                    Department = "Old Dept",
                    EmployeeId = "EMP001",
                    Role = "Doctor"
                };

            var dto =
                new DoctorDto
                {
                    Name = "New Name",
                    Department = "New Dept",
                    EmployeeId = "EMP002",
                    Role = "Consultant"
                };

            _repoMock.Setup(x =>
                x.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            // Act

            var result =
                await _service
                .UpdateDoctorAsync(
                    1,
                    dto
                );

            // Assert

            result.Success
                .Should().BeTrue();

            doctor.Name
                .Should().Be("New Name");

            doctor.Department
                .Should().Be("New Dept");
        }

        // =====================================
        // DELETE DOCTOR
        // =====================================

        [Fact]
        public async Task
        DeleteDoctorAsync_Should_Delete_Doctor()
        {
            // Arrange

            var doctor =
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Delete"
                };

            _repoMock.Setup(x =>
                x.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            _repoMock.Setup(x =>
                x.HasAssignedPatientsAsync(1))
                .ReturnsAsync(false);

            // Act

            var result =
                await _service
                .DeleteDoctorAsync(1);

            // Assert

            result.Success
                .Should().BeTrue();

            result.Message
                .Should().Be(
                    "Doctor deleted successfully"
                );

            _repoMock.Verify(
                x => x.RemoveAsync(doctor),
                Times.Once
            );
        }

        // =====================================
        // DELETE BLOCKED
        // =====================================

        [Fact]
        public async Task
        DeleteDoctorAsync_Should_Block_When_Patients_Exist()
        {
            // Arrange

            var doctor =
                new Doctor
                {
                    Id = 1,
                    Name = "Dr. Active"
                };

            _repoMock.Setup(x =>
                x.GetByIdAsync(1))
                .ReturnsAsync(doctor);

            _repoMock.Setup(x =>
                x.HasAssignedPatientsAsync(1))
                .ReturnsAsync(true);

            // Act

            var result =
                await _service
                .DeleteDoctorAsync(1);

            // Assert

            result.Success
                .Should().BeFalse();

            result.Message
                .Should().Contain(
                    "patients are assigned"
                );
        }

        // =====================================
        // UPDATE DISPLAY
        // =====================================

        [Fact]
        public async Task
        UpdateDisplayAsync_Should_Return_True()
        {
            // Arrange

            _repoMock.Setup(x =>
                x.UpdateDisplayAsync(
                    1,
                    "D1"
                ))
                .ReturnsAsync(true);

            // Act

            var result =
                await _service
                .UpdateDisplayAsync(
                    1,
                    "D1"
                );

            // Assert

            result.Should().BeTrue();
        }
    }
}