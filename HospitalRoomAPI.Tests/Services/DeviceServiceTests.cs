using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.Models;
using System.Collections.Generic;
using System.Linq;

public class DeviceServiceTests
{
	private readonly Mock<IDeviceRepository>
		_repoMock = new();

	private DeviceService GetService()
	{
		return new DeviceService(
			_repoMock.Object
		);
	}

	[Fact]
	public void GetDevices_Should_Return_Device_List()
	{
		// Arrange

		var service = GetService();

		var devices =
			new List<DisplayDevice>
			{
				new DisplayDevice
				{
					DeviceName = "TV-101",
					RoomNumber = "101",
					IsOnline = true,
					LastSeen = DateTime.Now
				},

				new DisplayDevice
				{
					DeviceName = "TV-102",
					RoomNumber = "102",
					IsOnline = false,
					LastSeen = DateTime.Now
				}
			};

		_repoMock.Setup(r =>
			r.GetAllDevices())
			.Returns(devices);

		// Act

		var result =
			service.GetDevices();

		// Assert

		Assert.NotNull(result);

		var list =
			Assert.IsAssignableFrom<
				IEnumerable<object>
			>(result);

		Assert.Equal(2, list.Count());
	}
}