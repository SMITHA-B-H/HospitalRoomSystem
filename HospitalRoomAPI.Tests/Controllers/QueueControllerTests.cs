using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

public class QueueControllerTests
{
    private readonly Mock<IQueueService> _serviceMock = new();

    private QueueController GetController()
    {
        return new QueueController(_serviceMock.Object);
    }

    // =====================================
    // ADD
    // =====================================

    [Fact]
    public async Task Add_Should_Return_Ok()
    {
        var controller = GetController();

        var dto = new QueueCreateDto
        {
            PatientName = "Ravi",
            DoctorId = 1
        };

        _serviceMock.Setup(s =>
            s.AddToQueueAsync(dto))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1
            });

        var result =
            await controller.Add(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // GET ALL
    // =====================================

    [Fact]
    public async Task GetAll_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.GetAllQueueAsync())
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // GET DOCTOR
    // =====================================

    [Fact]
    public async Task GetDoctor_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.GetDoctorQueueAsync(1))
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await controller.GetDoctor(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // GET STAGE
    // =====================================

    [Fact]
    public async Task GetStage_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.GetByStageAsync("LAB"))
            .ReturnsAsync(new List<QueueEntry>());

        var result =
            await controller.GetStage("LAB");

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // CALL NEXT SUCCESS
    // =====================================

    [Fact]
    public async Task CallNext_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.CallNextAsync(1))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1,
                Status = "InProgress"
            });

        var result =
            await controller.CallNext(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // CALL NEXT NO PATIENT
    // =====================================

    [Fact]
    public async Task CallNext_Should_Return_NoPatient_Message()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.CallNextAsync(1))
            .ReturnsAsync((QueueEntry?)null);

        var result =
            await controller.CallNext(1);

        var ok =
            Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(ok.Value);
    }

    // =====================================
    // COMPLETE
    // =====================================

    [Fact]
    public async Task Complete_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.CompleteAsync(1))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1
            });

        var result =
            await controller.Complete(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // COMPLETE NOT FOUND
    // =====================================

    [Fact]
    public async Task Complete_Should_Return_NotFound()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.CompleteAsync(1))
            .ReturnsAsync((QueueEntry?)null);

        var result =
            await controller.Complete(1);

        Assert.IsType<NotFoundResult>(result);
    }

    // =====================================
    // SKIP
    // =====================================

    [Fact]
    public async Task Skip_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.SkipAsync(1))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1
            });

        var result =
            await controller.Skip(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // MOVE
    // =====================================

    [Fact]
    public async Task Move_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.MoveStageAsync(1, "LAB"))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1,
                Stage = "LAB"
            });

        var result =
            await controller.Move(1, "LAB");

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // RESET
    // =====================================

    [Fact]
    public async Task Reset_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.ResetQueueAsync())
            .ReturnsAsync(
                "Queue Reset Successfully"
            );

        var result =
            await controller.Reset();

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // RECALL
    // =====================================

    [Fact]
    public async Task Recall_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.RecallAsync(1))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1,
                Status = "Waiting"
            });

        var result =
            await controller.Recall(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // DISPLAY QUEUE
    // =====================================

    [Fact]
    public async Task GetDisplayQueue_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.GetDisplayQueueAsync("D1"))
            .ReturnsAsync(new DisplayQueueDto());

        var result =
            await controller.GetDisplayQueue("D1");

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // CENTRAL DISPLAY
    // =====================================

    [Fact]
    public async Task GetCentralDisplay_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.GetCentralDisplayAsync())
            .ReturnsAsync(new List<CentralDisplayDto>());

        var result =
            await controller.GetCentralDisplay();

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // RESET DOCTOR QUEUE
    // =====================================

    [Fact]
    public async Task ResetDoctorQueue_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.ResetDoctorQueueAsync(1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = true,
                Message = "Reset Success"
            });

        var result =
            await controller.ResetDoctorQueue(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // EMERGENCY
    // =====================================

    [Fact]
    public async Task Emergency_Should_Return_Ok()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.EmergencyCallAsync(1))
            .ReturnsAsync(new QueueEntry
            {
                Id = 1,
                Status = "InProgress"
            });

        var result =
            await controller.Emergency(1);

        Assert.IsType<OkObjectResult>(result);
    }

    // =====================================
    // EMERGENCY NOT FOUND
    // =====================================

    [Fact]
    public async Task Emergency_Should_Return_NotFound()
    {
        var controller = GetController();

        _serviceMock.Setup(s =>
            s.EmergencyCallAsync(1))
            .ReturnsAsync((QueueEntry?)null);

        var result =
            await controller.Emergency(1);

        Assert.IsType<NotFoundResult>(result);
    }
}