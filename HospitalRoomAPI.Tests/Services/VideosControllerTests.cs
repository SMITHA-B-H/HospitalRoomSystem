using Xunit;
using Microsoft.AspNetCore.Mvc;
using HospitalRoomAPI.Controllers;

public class VideosControllerTests
{
    [Fact]
    public void GetVideo_Should_Return_NotFound_When_File_Missing()
    {
        var controller = new VideosController();

        var result = controller.GetVideo("invalid.mp4");

        Assert.IsType<NotFoundObjectResult>(result);
    }
}