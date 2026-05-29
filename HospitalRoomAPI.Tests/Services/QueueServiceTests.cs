using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QueueServiceTests
{
    private readonly Mock<IQueueRepository> _repoMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepoMock = new();

    private readonly Mock<IHubContext<RoomHub>> _hubMock = new();
    private readonly Mock<IHubClients> _clientsMock = new();
    private readonly Mock<IClientProxy> _clientProxyMock = new();

    private QueueService GetService()
    {
        _hubMock.Setup(h => h.Clients)
                .Returns(_clientsMock.Object);

        _clientsMock.Setup(c => c.All)
                    .Returns(_clientProxyMock.Object);

        _clientsMock.Setup(c => c.Group(It.IsAny<string>()))
                    .Returns(_clientProxyMock.Object);

        _clientProxyMock
            .Setup(c => c.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
            .Returns(Task.CompletedTask);

        return new QueueService(
            _repoMock.Object,
            _hubMock.Object,
            _doctorRepoMock.Object
        );
    }

    // =====================================
    // ADD TO QUEUE
    // =====================================

    [Fact]
    public async Task AddToQueue_Should_Create_Entry()
    {
        var service = GetService();

        var dto = new QueueCreateDto
        {
            PatientName = "Ravi",
            PhoneNumber = "9999999999",
            Department = "Cardiology",
            DoctorId = 1,
            DisplayNumber = "D1"
        };

        _repoMock.Setup(r =>
            r.GetLastActiveTokenByDoctorAsync(
                1,
                It.IsAny<DateTime>()))
            .ReturnsAsync(5);

        _doctorRepoMock.Setup(d =>
            d.GetByIdAsync(1))
            .ReturnsAsync(new Doctor
            {
                Id = 1,
                Name = "Dr. Surendra",
                HospitalId = 1
            });

        _doctorRepoMock.Setup(d =>
            d.GetHospitalNameAsync(1))
            .ReturnsAsync("Natus Hospital");

        _repoMock.Setup(r =>
            r.AddAsync(It.IsAny<QueueEntry>()))
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.AddToQueueAsync(dto);

        Assert.NotNull(result);

        Assert.Equal(6, result.TokenNumber);

        Assert.Equal("Waiting", result.Status);

        Assert.Equal("OPD", result.Stage);
    }

    // =====================================
    // CALL NEXT
    // =====================================

    [Fact]
    public async Task CallNext_Should_Set_InProgress()
    {
        var service = GetService();

        var queue = new List<QueueEntry>
        {
            new QueueEntry
            {
                Id = 1,
                Status = "Waiting"
            }
        };

        _repoMock.Setup(r =>
            r.GetDoctorQueueAsync(1))
            .ReturnsAsync(queue);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(queue);

        var result =
            await service.CallNextAsync(1);

        Assert.NotNull(result);

        Assert.Equal(
            "InProgress",
            result.Status
        );
    }

    // =====================================
    // COMPLETE
    // =====================================

    [Fact]
    public async Task Complete_Should_Set_Completed()
    {
        var service = GetService();

        var entry = new QueueEntry
        {
            Id = 1,
            Status = "InProgress"
        };

        _repoMock.Setup(r =>
            r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.CompleteAsync(1);

        Assert.NotNull(result);

        Assert.Equal(
            "Completed",
            result.Status
        );
    }

    // =====================================
    // SKIP
    // =====================================

    [Fact]
    public async Task Skip_Should_Set_Skipped()
    {
        var service = GetService();

        var entry = new QueueEntry
        {
            Id = 1,
            Status = "Waiting"
        };

        _repoMock.Setup(r =>
            r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.SkipAsync(1);

        Assert.NotNull(result);

        Assert.Equal(
            "Skipped",
            result.Status
        );
    }

    // =====================================
    // RECALL
    // =====================================

    [Fact]
    public async Task Recall_Should_Move_Back_To_Waiting()
    {
        var service = GetService();

        var entry = new QueueEntry
        {
            Id = 1,
            Status = "Skipped"
        };

        _repoMock.Setup(r =>
            r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.RecallAsync(1);

        Assert.NotNull(result);

        Assert.Equal(
            "Waiting",
            result.Status
        );
    }

    // =====================================
    // MOVE STAGE
    // =====================================

    [Fact]
    public async Task MoveStage_Should_Update_Stage()
    {
        var service = GetService();

        var entry = new QueueEntry
        {
            Id = 1,
            Stage = "OPD",
            Status = "InProgress"
        };

        _repoMock.Setup(r =>
            r.GetByIdAsync(1))
            .ReturnsAsync(entry);

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.MoveStageAsync(
                1,
                "LAB"
            );

        Assert.NotNull(result);

        Assert.Equal(
            "LAB",
            result.Stage
        );

        Assert.Equal(
            "Completed",
            result.Status
        );
    }

    // =====================================
    // RESET
    // =====================================

    [Fact]
    public async Task ResetQueue_Should_Return_Message()
    {
        var service = GetService();

        _repoMock.Setup(r =>
            r.ResetQueueAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.ResetQueueAsync();

        Assert.Equal(
            "Queue Reset Successfully",
            result
        );
    }

    // =====================================
    // EMERGENCY
    // =====================================

    [Fact]
    public async Task EmergencyCall_Should_Set_InProgress()
    {
        var service = GetService();

        var emergency = new QueueEntry
        {
            Id = 2,
            DoctorId = 1,
            Status = "Waiting"
        };

        var current = new QueueEntry
        {
            Id = 1,
            DoctorId = 1,
            Status = "InProgress"
        };

        _repoMock.Setup(r =>
            r.GetByIdAsync(2))
            .ReturnsAsync(emergency);

        _repoMock.Setup(r =>
            r.GetDoctorQueueAsync(1))
            .ReturnsAsync(new List<QueueEntry>
            {
                current,
                emergency
            });

        _repoMock.Setup(r =>
            r.SaveAsync())
            .Returns(Task.CompletedTask);

        _repoMock.Setup(r =>
            r.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await service.EmergencyCallAsync(2);

        Assert.NotNull(result);

        Assert.Equal(
            "InProgress",
            result.Status
        );

        Assert.Equal(
            "Waiting",
            current.Status
        );
    }
}