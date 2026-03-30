using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SettingsServiceTests
{
    private readonly Mock<ISettingsRepository> _repoMock = new();
    private readonly Mock<IDisplayService> _displayMock = new();

    private SettingsService GetService()
    {
        return new SettingsService(_repoMock.Object, _displayMock.Object);
    }

    // ================= SAVE SETTINGS =================

    [Fact]
    public async Task SaveSettings_Should_Create_New_Settings_When_Not_Exists()
    {
        var service = GetService();

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

        var result = await service.SaveSettings(dto, 1);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task SaveSettings_Should_Update_Existing_Settings()
    {
        var service = GetService();

        var existing = new Setting
        {
            Id = 1,
            HospitalId = 1,
            ScrollingMessage = "Old"
        };

        var dto = new SaveSettingsDto
        {
            ScrollingMessage = "Updated",
            AdsVolume = 30,
            ScrollingSpeed = 10
        };

        _repoMock.Setup(r => r.GetSettings(1, null, null))
                 .ReturnsAsync(existing);

        _repoMock.Setup(r => r.SaveSettings(existing))
                 .ReturnsAsync(existing);

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        var result = await service.SaveSettings(dto, 1);

        Assert.True(result.Success);
        Assert.Equal("Updated", existing.ScrollingMessage);
    }

    [Fact]
    public async Task SaveSettings_Should_Push_Update_To_All_Rooms()
    {
        var service = GetService();

        var dto = new SaveSettingsDto();

        _repoMock.Setup(r => r.GetSettings(1, null, null))
                 .ReturnsAsync(new Setting());

        _repoMock.Setup(r => r.SaveSettings(It.IsAny<Setting>()))
                 .ReturnsAsync(new Setting());

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101", "102" });

        await service.SaveSettings(dto, 1);

        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
        _displayMock.Verify(d => d.PushRoomUpdate("102"), Times.Once);
    }

    // ================= GET DISPLAY SETTINGS =================

    [Fact]
    public async Task GetDisplaySettings_Should_Return_Data_When_Room_Exists()
    {
        var service = GetService();

        var room = new Room
        {
            Id = 1,
            RoomNumber = "101",
            RoomName = "ICU",
            Floor = new Floor { HospitalId = 1 }
        };

        _repoMock.Setup(r => r.GetRoomWithDetails(1))
                 .ReturnsAsync(room);

        _repoMock.Setup(r => r.GetSettings(1, 1, null))
                 .ReturnsAsync(new Setting());

        _repoMock.Setup(r => r.GetVideos(1, 1, null))
                 .ReturnsAsync(new List<string>());

        _repoMock.Setup(r => r.GetAnnouncements(1, 1, null))
                 .ReturnsAsync(new List<PatientAnnouncement>());

        var result = await service.GetDisplaySettings(1);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetDisplaySettings_Should_Return_Fail_When_NoRoom()
    {
        var service = GetService();

        _repoMock.Setup(r => r.GetRoomWithDetails(It.IsAny<int>()))
                 .ReturnsAsync((Room?)null);

        var result = await service.GetDisplaySettings(1);

        Assert.False(result.Success);
    }

    // ================= UPLOAD LOGO =================

    [Fact]
    public async Task UploadLogo_Should_Return_Logo_Path()
    {
        var service = GetService();

        var fileMock = new Mock<IFormFile>();

        _repoMock.Setup(r => r.UploadLogo(fileMock.Object, 1))
                 .ReturnsAsync("/logos/test.png");

        var result = await service.UploadLogo(fileMock.Object, 1);

        Assert.True(result.Success);
        Assert.Equal("/logos/test.png", result.Data);
    }

    // ================= UPLOAD VIDEO =================

    [Fact]
    public async Task UploadVideo_Should_Save_And_Push_Update()
    {
        var service = GetService();

        var dto = new UploadVideoDto
        {
            File = new Mock<IFormFile>().Object
        };

        _repoMock.Setup(r => r.UploadVideo(dto, 1))
                 .ReturnsAsync("/videos/test.mp4");

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        var result = await service.UploadVideo(dto, 1);

        Assert.True(result.Success);

        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }

    // ================= DELETE VIDEO =================

    [Fact]
    public async Task DeleteVideo_Should_Delete_And_Push_Update()
    {
        var service = GetService();

        _repoMock.Setup(r => r.DeleteVideo("/videos/test.mp4"))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.GetRoomNumbers(null, null, 1))
                 .ReturnsAsync(new List<string> { "101" });

        var result = await service.DeleteVideo("/videos/test.mp4", 1);

        Assert.True(result.Success);

        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }
}