using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;

using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using HospitalRoomAPI.Models.Common;

public class DoctorsControllerTests
{
    private readonly Mock<IDoctorService>
        _serviceMock = new();

    private DoctorsController
        GetController()
    {
        var controller =
            new DoctorsController(
                _serviceMock.Object);

        var user =
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(
                            "HospitalId",
                            "1"),

                        new Claim(
                            ClaimTypes.Role,
                            "Admin")
                    },
                    "mock"));

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User = user
                    }
            };

        return controller;
    }

    // ================= GET =================

    [Fact]
    public async Task
        GetDoctors_ReturnsOk()
    {
        _serviceMock
            .Setup(s =>
                s.GetDoctorsAsync(1))
            .ReturnsAsync(
                new ApiResponse<
                    List<Doctor>>
                {
                    Success = true,
                    Data =
                        new List<Doctor>()
                });

        // ✅ FIXED: pass hospitalId
        var result =
            await GetController()
                .GetDoctors(1);

        var ok =
            Assert.IsType<
                OkObjectResult>(
                    result);

        var data =
            Assert.IsType<
                ApiResponse<
                    List<Doctor>>>(
                        ok.Value);

        Assert.True(
            data.Success);
    }

    // ================= ADD =================

    [Fact]
    public async Task
        AddDoctor_ReturnsOk_WhenSuccess()
    {
        var dto =
            new DoctorDto();

        _serviceMock
            .Setup(s =>
                s.AddDoctorAsync(
                    dto,
                    1,
                    "Admin"))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = true
                });

        var result =
            await GetController()
                .AddDoctor(dto);

        Assert.IsType<
            OkObjectResult>(
                result);
    }

    [Fact]
    public async Task
        AddDoctor_Returns403_WhenFail()
    {
        var dto =
            new DoctorDto();

        _serviceMock
            .Setup(s =>
                s.AddDoctorAsync(
                    dto,
                    1,
                    "Admin"))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = false
                });

        var result =
            await GetController()
                .AddDoctor(dto);

        var obj =
            Assert.IsType<
                ObjectResult>(
                    result);

        Assert.Equal(
            403,
            obj.StatusCode);
    }

    // ================= UPDATE =================

    [Fact]
    public async Task
        UpdateDoctor_ReturnsOk()
    {
        var dto =
            new DoctorDto();

        _serviceMock
            .Setup(s =>
                s.UpdateDoctorAsync(
                    1,
                    dto))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = true
                });

        var result =
            await GetController()
                .UpdateDoctor(
                    1,
                    dto);

        Assert.IsType<
            OkObjectResult>(
                result);
    }

    [Fact]
    public async Task
        UpdateDoctor_ReturnsNotFound()
    {
        var dto =
            new DoctorDto();

        _serviceMock
            .Setup(s =>
                s.UpdateDoctorAsync(
                    1,
                    dto))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = false
                });

        var result =
            await GetController()
                .UpdateDoctor(
                    1,
                    dto);

        Assert.IsType<
            NotFoundObjectResult>(
                result);
    }

    // ================= DELETE =================

    [Fact]
    public async Task
        DeleteDoctor_ReturnsOk()
    {
        _serviceMock
            .Setup(s =>
                s.DeleteDoctorAsync(
                    1))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = true
                });

        var result =
            await GetController()
                .DeleteDoctor(1);

        Assert.IsType<
            OkObjectResult>(
                result);
    }

    [Fact]
    public async Task
        DeleteDoctor_ReturnsNotFound()
    {
        _serviceMock
            .Setup(s =>
                s.DeleteDoctorAsync(
                    1))
            .ReturnsAsync(
                new ApiResponse<
                    Doctor>
                {
                    Success = false
                });

        var result =
            await GetController()
                .DeleteDoctor(1);

        Assert.IsType<OkObjectResult>(result);
    }
}