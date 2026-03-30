using Xunit;
using Moq;
using HospitalRoomAPI.Services;
using HospitalRoomAPI.Repositories;
using HospitalRoomAPI.DTOs;
using HospitalRoomAPI.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HospitalRoomAPI.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IAuthRepository> _repoMock;
        private readonly Mock<IEmailService> _emailMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _repoMock = new Mock<IAuthRepository>();
            _emailMock = new Mock<IEmailService>();

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    // ✅ FIXED KEY (>= 32 chars)
                    {"Jwt:Key", "THIS_IS_A_VERY_SECURE_SECRET_KEY_1234567890"},
                    {"Jwt:Issuer", "test"},
                    {"Jwt:Audience", "test"},
                    {"Jwt:DurationInMinutes", "60"}
                })
                .Build();

            _service = new AuthService(
                _repoMock.Object,
                config,
                _emailMock.Object
            );
        }

        // ================= REGISTER =================

        [Fact]
        public async Task Register_Should_Create_User()
        {
            _repoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            // ✅ IMPORTANT MOCKS
            _repoMock.Setup(r => r.AddHospitalAsync(It.IsAny<Hospital>()))
                     .ReturnsAsync((Hospital h) => h);

            _repoMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

            _repoMock.Setup(r => r.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            var dto = new RegisterHospitalDto
            {
                Email = "test@mail.com",
                Password = "123",
                HospitalName = "H1",
                Address = "Test Address"
            };

            var result = await _service.RegisterHospitalAsync(dto);

            Assert.True(result.Success);

            _repoMock.Verify(r => r.AddHospitalAsync(It.IsAny<Hospital>()), Times.Once);
            _repoMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        // ================= LOGIN SUCCESS =================

        [Fact]
        public async Task Login_Should_Return_Token()
        {
            var user = new User
            {
                Id = 1,
                Email = "test@mail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Role = "SuperAdmin",
                HospitalId = 1
            };

            _repoMock.Setup(r => r.GetUserByEmailAsync("test@mail.com"))
                     .ReturnsAsync(user);

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "test@mail.com",
                Password = "123"
            });

            Assert.True(result.Success);
            Assert.NotNull(result.Data); // token exists
        }

        // ================= LOGIN FAIL =================

        [Fact]
        public async Task Login_Should_Fail_When_Invalid()
        {
            _repoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "x",
                Password = "y"
            });

            Assert.False(result.Success);
        }
    }
}