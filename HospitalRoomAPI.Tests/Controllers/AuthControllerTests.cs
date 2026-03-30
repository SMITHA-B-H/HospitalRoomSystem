using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models.Common;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _serviceMock = new();

    private AuthController GetController()
    {
        return new AuthController(_serviceMock.Object);
    }

    [Fact]
    public async Task RegisterHospital_ReturnsOk_WhenSuccess()
    {
        var controller = GetController();

        var dto = new RegisterHospitalDto
        {
            HospitalName = "Test Hospital",
            Email = "test@mail.com",
            Password = "123456"
        };

        _serviceMock.Setup(x => x.RegisterHospitalAsync(dto))
            .ReturnsAsync(new ApiResponse<object>   // ✅ FIXED TYPE
            {
                Success = true,
                Data = "token",
                Message = "Success"
            });

        var result = await controller.Register(dto); // ✅ FIXED METHOD

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(ok.Value);

        Assert.True(response.Success);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenFailed()
    {
        var controller = GetController();

        var dto = new LoginDto
        {
            Email = "wrong@mail.com",
            Password = "wrong"
        };

        _serviceMock.Setup(x => x.LoginAsync(dto))
            .ReturnsAsync(new ApiResponse<object>   // ✅ FIXED TYPE
            {
                Success = false,
                Message = "Invalid",
                Data = null
            });

        var result = await controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}