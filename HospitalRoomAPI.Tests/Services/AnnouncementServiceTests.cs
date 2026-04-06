using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnnouncementServiceTests
{
    private readonly Mock<IAnnouncementRepository> _repoMock = new();
    private readonly Mock<IDisplayService> _displayMock = new();
    private readonly Mock<IFileStorageService> _fileServiceMock = new();

    private AnnouncementService GetService()
        => new(_repoMock.Object, _displayMock.Object, _fileServiceMock.Object);

    [Fact]
    public async Task Create_Should_Succeed()
    {
        var service = GetService();

        var model = new PatientAnnouncement
        {
            ExpiryHours = 1,
            RoomId = 1
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<PatientAnnouncement>()))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        // PushUpdate dependencies
        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });
        _repoMock.Setup(r => r.GetRoomNumbersByFloorId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });
        _repoMock.Setup(r => r.GetRoomNumbersByHospitalId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });
        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        var result = await service.CreateAsync(model, 1);

        Assert.True(result.Success);
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }

    [Fact]
    public async Task Delete_Should_Call_Display()
    {
        var service = GetService();

        var announcement = new PatientAnnouncement
        {
            Id = 1,
            RoomId = 1
        };

        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(announcement);

        _repoMock.Setup(r => r.DeleteAsync(It.IsAny<PatientAnnouncement>()))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        var result = await service.DeleteAsync(1);

        Assert.True(result.Success);
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }

    [Fact]
    public async Task UploadPoster_Should_Return_Url()
    {
        var service = GetService();

        var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();

        _fileServiceMock.Setup(f => f.UploadAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                        .ReturnsAsync("https://drive.google.com/uc?id=123");

        var result = await service.UploadPoster(fileMock.Object);

        Assert.True(result.Success);
        Assert.Equal("https://drive.google.com/uc?id=123", result.Data);
    }

    [Fact]
    public async Task UploadVideo_Should_Return_Url()
    {
        var service = GetService();

        var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();

        _fileServiceMock.Setup(f => f.UploadAsync(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
                        .ReturnsAsync("https://drive.google.com/uc?id=123");

        var result = await service.UploadVideo(fileMock.Object);

        Assert.True(result.Success);
        Assert.Equal("https://drive.google.com/uc?id=123", result.Data);
    }
}