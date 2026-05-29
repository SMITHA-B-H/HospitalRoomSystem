using Microsoft.EntityFrameworkCore;
using HospitalRoomAPI.Data;
using HospitalRoomAPI.Models;

namespace HospitalRoomAPI.Repositories
{
    public class FloorRepository : IFloorRepository
    {
        private readonly AppDbContext _context;

        public FloorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Floor>> GetFloorsAsync(int hospitalId)
        {
            return await _context.Floors
                .Include(f => f.Users)
                .Where(f => f.HospitalId == hospitalId)
                .ToListAsync();
        }

        public async Task<Floor?> GetFloorByIdAsync(int id)
        {
            return await _context.Floors
                .Include(f => f.Users)
                .Include(f => f.Rooms)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFloorAsync(Floor floor)
        {
            await _context.Floors.AddAsync(floor);
        }

        public async Task DeleteFloorAsync(Floor floor)
        {
            _context.Floors.Remove(floor);
        }

        public async Task<List<string>> GetRoomNumbersByFloorIdAsync(int floorId)
        {
            return await _context.Rooms
                .Where(r => r.FloorId == floorId)
                .Select(r => r.RoomNumber)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}