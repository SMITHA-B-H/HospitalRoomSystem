using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using System;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsRepository> _repoMock = new();
    private readonly Mock<IDisplayService> _displayMock = new();
    private readonly Mock<IFileStorageService> _fileMock = new();

    // ? Create InMemory DB
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB per test
            .Options;

        return new AppDbContext(options);
    }

    private SettingsService GetService(AppDbContext context)
    {
        return new SettingsService(
            _repoMock.Object,
            _displayMock.Object,
            _fileMock.Object,
            context   // ? FIXED
        );
    }

    // ================= SAVE SETTINGS =================
    [Fact]
    public async Task SaveSettings_Should_Create_New_Settings_When_Not_Exists()
    {
        var context = GetDbContext();
        var service = GetService(context);

        var dto = new SaveSettingsDto
        {
            ScrollingMessage = "Hello",
            AdsVolume = 20,
            ScrollingSpeed = 5
        };

        _repoMock.Setup(r => r.GetSettings(1, null, null))
                 .ReturnsAsync((Setting?)null);

        _repoMock.Setup(r => r.SaveSettings(It.IsAny<Setting>()))
                 .ReturnsAsync(new Setting());

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        var result = await service.SaveSettings(dto, 1);

        Assert.True(result.Success);
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }

    // ================= UPLOAD LOGO =================
    [Fact]
    public async Task UploadLogo_Should_Return_Url()
    {
        var context = GetDbContext();
        var service = GetService(context);

        var fileMock = new Mock<IFormFile>();

        _fileMock.Setup(f => f.UploadAsync(fileMock.Object))
                 .ReturnsAsync("https://drive.google.com/uc?id=123");

        _repoMock.Setup(r => r.SaveLogo(It.IsAny<string>(), It.IsAny<int>()))
                 .Returns(Task.CompletedTask);

        var result = await service.UploadLogo(fileMock.Object, 1);

        Assert.True(result.Success);
        Assert.Equal("https://drive.google.com/uc?id=123", result.Data);
    }

    // ================= UPLOAD VIDEO =================
    [Fact]
    public async Task UploadVideo_Should_Save_And_Push_Update()
    {
        var context = GetDbContext();
        var service = GetService(context);

        var dto = new UploadVideoDto
        {
            File = new Mock<IFormFile>().Object
        };

        _fileMock.Setup(f => f.UploadAsync(dto.File))
                 .ReturnsAsync("https://drive.google.com/uc?id=123");

        _repoMock.Setup(r => r.SaveVideo(It.IsAny<string>(), It.IsAny<int>(), dto))
                 .ReturnsAsync("https://drive.google.com/uc?id=123");

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        var result = await service.UploadVideo(dto, 1);

        Assert.True(result.Success);
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }

    // ================= DELETE VIDEO =================
    [Fact]
    public async Task DeleteVideo_Should_Delete_And_Push_Update()
    {
        var context = GetDbContext();
        var service = GetService(context);

        var url = "https://drive.google.com/uc?id=123";

        _repoMock.Setup(r => r.DeleteVideo(url))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        _displayMock.Setup(d => d.PushRoomUpdate(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

        _fileMock.Setup(f => f.DeleteAsync(It.IsAny<string>()))
                 .Returns(Task.CompletedTask);

        var result = await service.DeleteVideo(url, 1);

        Assert.True(result.Success);
        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }
}