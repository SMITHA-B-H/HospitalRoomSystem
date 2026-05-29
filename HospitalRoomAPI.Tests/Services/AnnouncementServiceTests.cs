using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class AnnouncementServiceTests
{
    private readonly Mock<IAnnouncementRepository> _repoMock = new();
    private readonly Mock<IDisplayService> _displayMock = new();

    private AnnouncementService GetService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "StoragePath", Path.GetTempPath() } // safe temp folder
            })
            .Build();

        return new AnnouncementService(_repoMock.Object, _displayMock.Object, config);
    }

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

        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(It.IsAny<int>()))
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

    // ? FIXED (LOCAL STORAGE VERSION)
    [Fact]
    public async Task UploadPoster_Should_Return_Url()
    {
        var service = GetService();

        var fileMock = new Mock<IFormFile>();

        var content = "dummy content";
        var fileName = "test.jpg";
        var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns((Stream stream, CancellationToken token) =>
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                    return Task.CompletedTask;
                });

        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(new PatientAnnouncement
                 {
                     Id = 1,
                     RoomId = 1
                 });

        _repoMock.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        var result = await service.UploadPoster(fileMock.Object, 1);

        Assert.True(result.Success);
        Assert.Contains("/files/announcements/posters/", result.Data);
    }

    // ? FIXED (LOCAL STORAGE VERSION)
    [Fact]
    public async Task UploadVideo_Should_Return_Url()
    {
        var service = GetService();

        var fileMock = new Mock<IFormFile>();

        var content = "dummy video";
        var fileName = "test.mp4";

        var ms = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(content));

        fileMock.Setup(f => f.OpenReadStream())
                .Returns(ms);

        fileMock.Setup(f => f.FileName)
                .Returns(fileName);

        fileMock.Setup(f => f.Length)
                .Returns(ms.Length);

        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Returns((Stream stream, CancellationToken token) =>
                {
                    ms.Position = 0;
                    ms.CopyTo(stream);
                    return Task.CompletedTask;
                });

        // ================= MOCK ANNOUNCEMENT =================

        _repoMock.Setup(r => r.GetByIdAsync(1))
                 .ReturnsAsync(new PatientAnnouncement
                 {
                     Id = 1,
                     RoomId = 1
                 });

        _repoMock.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(It.IsAny<int>()))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        // ================= CALL =================

        var result = await service.UploadVideo(fileMock.Object, 1);

        // ================= ASSERT =================

        Assert.True(result.Success);

        Assert.Contains(
            "/files/announcements/videos/",
            result.Data
        );
    }
}