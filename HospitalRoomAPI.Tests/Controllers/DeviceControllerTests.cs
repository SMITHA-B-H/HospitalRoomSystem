using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;
using HospitalRoomAPI.Services;

public class DeviceControllerTests
{
    private readonly Mock<IDeviceService> _serviceMock = new();

    private DeviceController GetController()
    {
        return new DeviceController(
            _serviceMock.Object
        );
    }

    [Fact]
    public void GetDevices_Should_Return_Ok()
    {
        // Arrange
        var controller = GetController();

        var devices = new List<string>
        {
            "Device1",
            "Device2"
        };

        _serviceMock.Setup(s =>
            s.GetDevices())
            .Returns(devices);

        // Act
        var result =
            controller.GetDevices();

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(result);

        Assert.NotNull(okResult.Value);
    }
}