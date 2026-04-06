using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

public class SettingsControllerTests
{
    private readonly Mock<ISettingsService> _serviceMock = new();

    private SettingsController GetController()
    {
        var controller = new SettingsController(_serviceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("HospitalId", "1")
        }, "mock"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    // ================= GET DISPLAY =================

    [Fact]
    public async Task GetDisplaySettings_ReturnsOk()
    {
        _serviceMock
            .Setup(s => s.GetDisplaySettings(1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.GetDisplaySettings(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ApiResponse<object>>(ok.Value);

        Assert.True(data.Success);
    }

    // ================= SAVE SETTINGS =================

    [Fact]
    public async Task SaveSettings_ReturnsOk()
    {
        var dto = new SaveSettingsDto();

        _serviceMock
            .Setup(s => s.SaveSettings(dto, 1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.SaveSettings(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    // ================= UPLOAD LOGO =================

    [Fact]
    public async Task UploadLogo_ReturnsOk()
    {
        var fileMock = new Mock<IFormFile>();

        _serviceMock
            .Setup(s => s.UploadLogo(fileMock.Object, 1))
            .ReturnsAsync(new ApiResponse<string>
            {
                Success = true,
                Data = "logo.png"
            });

        var controller = GetController();

        var result = await controller.UploadLogo(fileMock.Object);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ApiResponse<string>>(ok.Value);

        Assert.True(data.Success);
    }

    // ================= UPLOAD VIDEO =================

    [Fact]
    public async Task UploadVideo_ReturnsOk()
    {
        var dto = new UploadVideoDto();

        _serviceMock
            .Setup(s => s.UploadVideo(dto, 1))
            .ReturnsAsync(new ApiResponse<string>
            {
                Success = true,
                Data = "video.mp4"
            });

        var controller = GetController();

        var result = await controller.UploadVideo(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ApiResponse<string>>(ok.Value);

        Assert.True(data.Success);
    }

    // ================= DELETE VIDEO =================

    [Fact]
    public async Task DeleteVideo_ReturnsOk()
    {
        string path = "/videos/test.mp4";

        _serviceMock
            .Setup(s => s.DeleteVideo(path, 1))
            .ReturnsAsync(new ApiResponse<object>
            {
                Success = true
            });

        var controller = GetController();

        var result = await controller.DeleteVideo(path);

        var ok = Assert.IsType<OkObjectResult>(result);
        var data = Assert.IsType<ApiResponse<object>>(ok.Value);

        Assert.True(data.Success);
    }
}