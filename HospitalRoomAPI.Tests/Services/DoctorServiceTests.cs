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
    private readonly Mock<IFileStorageService> _fileMock = new();

    public DoctorServiceTests()
    {
        _repoMock = new Mock<IDoctorRepository>();
        _displayMock = new Mock<IDisplayService>();
        _fileMock = new Mock<IFileStorageService>();
        _service = new DoctorService(_repoMock.Object, _displayMock.Object, _fileMock.Object);
        
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
    public async Task AddDoctor_Should_Succeed()
    {
        var dto = new DoctorDto
        {
            Name = "Dr Test",
            Department = "Cardio"
        };

        _fileMock.Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
                 .ReturnsAsync("https://drive.google.com/test");

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Doctor>()))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.SaveChangesAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetDoctorRoomNumbersAsync(It.IsAny<int>()))
                 .ReturnsAsync(new List<string>());

        var result = await _service.AddDoctorAsync(dto, 1, "SuperAdmin");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateDoctor_Should_Succeed()
    {
        var service = _service;

        var doctor = new Doctor { Id = 1 };

        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(doctor);

        _repoMock.Setup(r => r.SaveChangesAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetDoctorRoomNumbersAsync(1))
                 .ReturnsAsync(new List<string>());

        var result = await service.UpdateDoctorAsync(1, new DoctorDto());

        Assert.True(result.Success);
    }
}