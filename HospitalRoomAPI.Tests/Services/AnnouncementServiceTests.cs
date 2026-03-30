using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;

public class AnnouncementServiceTests
{
    private readonly Mock<IAnnouncementRepository> _repoMock = new();
    private readonly Mock<IDisplayService> _displayMock = new();

    private AnnouncementService GetService()
        => new(_repoMock.Object, _displayMock.Object);

    [Fact]
    public async Task Create_Should_Succeed()
    {
        var service = GetService();

        var model = new PatientAnnouncement
        {
            ExpiryHours = 1
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<PatientAnnouncement>()))
                 .Returns(Task.CompletedTask);

        _repoMock.Setup(r => r.SaveAsync())
                 .Returns(Task.CompletedTask);

        var result = await service.CreateAsync(model, 1);

        Assert.True(result.Success);
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

        _repoMock.Setup(r => r.GetRoomNumbersByRoomId(1))
                 .ReturnsAsync(new List<string> { "101" });

        var result = await service.DeleteAsync(1);

        Assert.True(result.Success);

        _displayMock.Verify(d => d.PushRoomUpdate("101"), Times.Once);
    }
}