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
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<QueueEntry> QueueEntries { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<AdsVideo> AdsVideos { get; set; }
        public DbSet<PatientAnnouncement> PatientAnnouncements { get; set; }
        public DbSet<DisplayDevice> DisplayDevices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================
            // ONE SUPER ADMIN PER HOSPITAL
            // ================================
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.HospitalId, u.Role })
                .HasFilter("[Role] = 'SuperAdmin'")
                .IsUnique();

            // ================================
            // HOSPITAL → FLOORS
            // ================================
            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Hospital)
                .WithMany(h => h.Floors)
                .HasForeignKey(f => f.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================================
            // FLOOR → ROOMS
            // ================================
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(r => r.FloorId)
                .OnDelete(DeleteBehavior.Cascade);

            // ❌ REMOVE THIS ENTIRE BLOCK (CAUSE OF ERROR)
            // modelBuilder.Entity<Room>()
            //     .HasOne(r => r.Floor.Hospital)
            //     .WithMany()
            //     .HasForeignKey(r => r.Floor.HospitalId)
            //     .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // ROOM → BEDS
            // ================================
            modelBuilder.Entity<Bed>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Beds)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================================
            // BED → PATIENT
            // ================================
            modelBuilder.Entity<Bed>()
                .HasOne(b => b.Patient)
                .WithOne()
                .HasForeignKey<Bed>(b => b.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // ================================
            // PATIENT → DOCTOR
            // ================================
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.Doctor)
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // ================================
            // DOCTOR → HOSPITAL
            // ================================
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Hospital)
                .WithMany(h => h.Doctors)
                .HasForeignKey(d => d.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            // ================================
            // DOCTOR → QueueEntry
            // ================================
            modelBuilder.Entity<QueueEntry>()
                .HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}