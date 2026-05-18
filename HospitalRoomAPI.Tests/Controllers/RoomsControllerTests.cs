using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;
using HospitalRoomAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public class RoomsControllerTests
{
    private readonly Mock<IRoomService> _serviceMock = new();

    private RoomsController GetController()
    {
        var controller = new RoomsController(_serviceMock.Object);

        // Fake user claims for hospital and floor
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("HospitalId", "1"),
            new Claim("FloorId", "1"),
            new Claim(ClaimTypes.Role, "Admin")
        }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        return controller;
    }

    [Fact]
    public async Task GetRooms_ReturnsOk()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.GetRoomsAsync("Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = true,
                Data = new List<object>()
            });

        var result = await controller.GetRooms();

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task CreateRoom_ReturnsOk()
    {
        var controller = GetController();
        var room = new Room();

        _serviceMock.Setup(x => x.CreateRoomAsync(room, "Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<Room>
            {
                Success = true,
                Data = room
            });

        var result = await controller.CreateRoom(room);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Room>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task AssignPatient_ReturnsOk()
    {
        var controller = GetController();
        var dto = new AssignPatientDto();

        _serviceMock.Setup(x => x.AssignPatientAsync(dto))
            .ReturnsAsync(new ApiResponse<Patient>
            {
                Success = true,
                Data = new Patient()
            });

        var result = await controller.AssignPatient(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Patient>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DischargePatient_ReturnsOk()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.DischargePatientAsync(1))
            .ReturnsAsync(new ApiResponse<object> { Success = true });

        var result = await controller.DischargePatient(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetRoomsByFloor_ReturnsOk()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.GetRoomsByFloorAsync(1))
            .ReturnsAsync(new List<object> { new Room() });

        var result = await controller.GetRoomsByFloor(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<object>>(ok.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task DeleteRoom_ReturnsOk()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.DeleteRoomAsync(1, "Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<object> { Success = true });

        var result = await controller.DeleteRoom(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateRoom_ReturnsOk()
    {
        var controller = GetController();
        var room = new Room();

        _serviceMock.Setup(x => x.UpdateRoomAsync(1, room, "Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<Room>
            {
                Success = true,
                Data = room
            });

        var result = await controller.UpdateRoom(1, room);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<Room>>(ok.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteRoom_WhenRoomExistsInFloor_ReturnsBadRequest()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.DeleteRoomAsync(1, "Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = false,
                Message = "Room cannot be deleted because beds exist."
            });

        var result = await controller.DeleteRoom(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        var message = Assert.IsType<string>(badRequest.Value);

        Assert.Equal("Room cannot be deleted because beds exist.", message);
    }

    [Fact]
    public async Task DeleteRoom_WhenPatientExistsInRoom_ReturnsBadRequest()
    {
        var controller = GetController();

        _serviceMock.Setup(x => x.DeleteRoomAsync(1, "Admin", 1, 1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = false,
                Message = "Room cannot be deleted because patient is present."
            });

        var result = await controller.DeleteRoom(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);

        var message = Assert.IsType<string>(badRequest.Value);

        Assert.Equal(
            "Room cannot be deleted because patient is present.",
            message);
    }


}