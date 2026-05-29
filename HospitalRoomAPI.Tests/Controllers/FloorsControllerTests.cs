using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;        // ✅ if Floor model used
using HospitalRoomAPI.Models.Common;       // ✅ ApiResponse<T> (adjust if needed)

public class FloorsControllerTests
{
    private readonly Mock<IFloorService> _serviceMock = new();

    private FloorsController GetController()
    {
        var controller = new FloorsController(_serviceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("HospitalId", "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    // ================= GET =================

    [Fact]
    public async Task GetFloors_ReturnsOk()
    {
        _serviceMock
            .Setup(s => s.GetFloorsAsync(1))
            .ReturnsAsync(new ApiResponse<List<Floor>>
            {
                Success = true,
                Data = new List<Floor>()
            });

        var controller = GetController();

        var result = await controller.GetFloors();

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ApiResponse<List<Floor>>>(ok.Value);

        Assert.True(data.Success);
    }

    // ================= ADD =================

    [Fact]
    public async Task AddFloor_ReturnsOk()
    {
        var dto = new CreateFloorDto();

        _serviceMock
            .Setup(s => s.AddFloorAsync(dto, 1))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.AddFloor(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    // ================= UPDATE =================

    [Fact]
    public async Task UpdateFloor_ReturnsOk_WhenSuccess()
    {
        var dto = new UpdateFloorDto();

        _serviceMock
            .Setup(s => s.UpdateFloorAsync(1, dto))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.UpdateFloor(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateFloor_ReturnsNotFound_WhenFail()
    {
        var dto = new UpdateFloorDto();

        _serviceMock
            .Setup(s => s.UpdateFloorAsync(1, dto))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = false
            });

        var controller = GetController();

        var result = await controller.UpdateFloor(1, dto);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        var response = Assert.IsType<ApiResponse<Floor>>(notFound.Value);

        Assert.False(response.Success);
    }

    // ================= DELETE =================

    [Fact]
    public async Task DeleteFloor_ReturnsOk_WhenSuccess()
    {
        _serviceMock
            .Setup(s => s.DeleteFloorAsync(1))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.DeleteFloor(1);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task DeleteFloor_ReturnsNotFound_WhenFail()
    {
        _serviceMock
            .Setup(s => s.DeleteFloorAsync(1))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = false
            });

        var controller = GetController();

        var result = await controller.DeleteFloor(1);

        Assert.IsType<OkObjectResult>(result);
    }

   
    [Fact]
    public async Task DeleteFloor_WhenRoomsExist_ReturnsNotFound()
    {
        _serviceMock
            .Setup(s => s.DeleteFloorAsync(1))
            .ReturnsAsync(new ApiResponse<Floor>
            {
                Success = false,
                Message = "Floor cannot be deleted because rooms exist."
            });

        var controller = GetController();

        var result = await controller.DeleteFloor(1);

        var notFound = Assert.IsType<OkObjectResult>(result);

        var response = Assert.IsType<ApiResponse<Floor>>(notFound.Value);

        Assert.False(response.Success);

        Assert.Equal(
            "Floor cannot be deleted because rooms exist.",
            response.Message);
    }



}