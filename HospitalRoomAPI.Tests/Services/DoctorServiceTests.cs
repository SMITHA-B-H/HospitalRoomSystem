using Xunit;
using Moq;
using FluentAssertions;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _repoMock;
    private readonly Mock<IDisplayService> _displayMock;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _repoMock = new Mock<IDoctorRepository>();
        _displayMock = new Mock<IDisplayService>();

        _service = new DoctorService(_repoMock.Object, _displayMock.Object);
    }

    [Fact]
    public async Task GetDoctors_Should_Return_List()
    {
        _repoMock.Setup(r => r.GetDoctorsAsync(1))
            .ReturnsAsync(new List<Doctor> { new Doctor { Name = "Dr A" } });

        var result = await _service.GetDoctorsAsync(1);

        result.Success.Should().BeTrue();
        result.Data!.Count.Should().Be(1);
    }

    [Fact]
    public async Task AddDoctor_Should_Fail_If_Not_Admin()
    {
        var dto = new DoctorDto { Name = "Dr A", Department = "Cardio" };

        var result = await _service.AddDoctorAsync(dto, 1, "User", "wwwroot");

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteDoctor_Should_Remove_And_Call_Display()
    {
        var doctor = new Doctor { Id = 1 };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _repoMock.Setup(r => r.GetDoctorRoomNumbersAsync(1))
            .ReturnsAsync(new List<string> { "101" });

        var result = await _service.DeleteDoctorAsync(1);

        result.Success.Should().BeTrue();
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }
}