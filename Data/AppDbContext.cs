using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
    }
}