using Xunit;
using Moq;
using FluentAssertions;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _repoMock;
    private readonly Mock<IDisplayService> _displayMock;
    private readonly DoctorService _service;

    public DoctorServiceTests()
    {
        _repoMock = new Mock<IDoctorRepository>();
        _displayMock = new Mock<IDisplayService>();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "StoragePath", Path.GetTempPath() }
            })
            .Build();

        _service = new DoctorService(_repoMock.Object, _displayMock.Object, config);
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
        var doctor = new Doctor { Id = 1 };

        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(doctor);

        _repoMock.Setup(r => r.SaveChangesAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetDoctorRoomNumbersAsync(1))
                 .ReturnsAsync(new List<string>());

        var result = await _service.UpdateDoctorAsync(1, new DoctorDto());

        Assert.True(result.Success);
    }

    // ? NEW TEST (important for your new logic)
    [Fact]
    public async Task AddDoctor_WithPhoto_Should_Save_Image()
    {
        var fileMock = new Mock<IFormFile>();

        var content = "dummy image";
        var fileName = "doc.jpg";
        var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns((Stream stream, CancellationToken token) =>
                {
                    ms.CopyTo(stream);
                    return Task.CompletedTask;
                });

        var dto = new DoctorDto
        {
            Name = "Dr Image",
            Department = "Ortho",
            Photo = fileMock.Object
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Doctor>()))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.SaveChangesAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetDoctorRoomNumbersAsync(It.IsAny<int>()))
                 .ReturnsAsync(new List<string>());

        var result = await _service.AddDoctorAsync(dto, 1, "SuperAdmin");

        Assert.True(result.Success);
        Assert.Contains("/files/doctors/", result.Data!.PhotoUrl);
    }
}