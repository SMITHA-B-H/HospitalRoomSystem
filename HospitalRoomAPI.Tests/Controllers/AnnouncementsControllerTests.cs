using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs; // ? ADD THIS

public class AnnouncementsControllerTests
{
    private readonly Mock<IAnnouncementService> _serviceMock = new();

    private AnnouncementsController GetController() => new(_serviceMock.Object);

    [Fact]
    public async Task Create_ReturnsOk()
    {
        var model = new PatientAnnouncement();

        _serviceMock.Setup(s => s.CreateAsync(model, 1))
            .ReturnsAsync(new ApiResponse<PatientAnnouncement>
            {
                Success = true,
                Data = model
            });

        var result = await GetController().Create(model);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<PatientAnnouncement>>(ok.Value);
    }

    [Fact]
    public async Task GetRoom_ReturnsOk()
    {
        var roomId = 1;

        _serviceMock.Setup(s => s.GetRoomAsync(roomId))
            .ReturnsAsync(new ApiResponse<List<PatientAnnouncement>>
            {
                Success = true,
                Data = new List<PatientAnnouncement>()
            });

        var result = await GetController().GetRoom(roomId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<List<PatientAnnouncement>>>(ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new ApiResponse<List<PatientAnnouncement>>
            {
                Success = true,
                Data = new List<PatientAnnouncement>()
            });

        var result = await GetController().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<List<PatientAnnouncement>>>(ok.Value);
    }

    [Fact]
    public async Task Delete_ReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(new ApiResponse<PatientAnnouncement>
            {
                Success = true
            });

        var result = await GetController().Delete(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<PatientAnnouncement>>(ok.Value);
    }

    [Fact]
    public async Task Deactivate_ReturnsOk()
    {
        _serviceMock.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(new ApiResponse<PatientAnnouncement>
            {
                Success = true
            });

        var result = await GetController().Deactivate(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<PatientAnnouncement>>(ok.Value);
    }

    // ? FIXED TEST
    [Fact]
    public async Task GetPatients_ReturnsOk()
    {
        _serviceMock.Setup(s => s.GetPatientsByRoom(1))
            .ReturnsAsync(new ApiResponse<List<PatientDto>>   // ?? FIX HERE
            {
                Success = true,
                Data = new List<PatientDto>()
            });

        var result = await GetController().GetPatients(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ApiResponse<List<PatientDto>>>(ok.Value); // ? also update assertion
    }
}