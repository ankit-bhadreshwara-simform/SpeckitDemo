using SpeckitDemo.Data.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpeckitDemo.Data.Mapping;

public sealed class MemberMap : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(member => member.Id);
        builder.Property(member => member.Id).ValueGeneratedOnAdd();

        builder.Property(member => member.Username)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(member => member.Username).IsUnique();

        builder.Property(member => member.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(member => member.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(member => member.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(member => member.Email)
            .IsRequired()
            .HasMaxLength(255);
        builder.HasIndex(member => member.Email).IsUnique();

        builder.Property(member => member.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(member => member.CreatedDate)
            .IsRequired();

        var openedOn = new DateTime(2026, 3, 14, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Member
            {
                Id = 1,
                Username = "mira",
                FirstName = "Mira",
                LastName = "Calder",
                Email = "mira.calder@speckitdemo.local",
                Address = "18 Harbor Lane, Port Merrin",
                PasswordHash = "$2a$12$MMliak12hVM4lTs0KW5FdeYYiEN6wJUGuEnsoI9EpEGg0zlmUDK6.",
                CreatedDate = openedOn
            },
            new Member
            {
                Id = 2,
                Username = "nolan",
                FirstName = "Nolan",
                LastName = "Briggs",
                Email = "nolan.briggs@speckitdemo.local",
                Address = "42 Cedar Court, West Halden",
                PasswordHash = "$2a$12$YXiRCZVz8j0H41rVo/x9xOaJNU6qASaHD37Qc3ilpBvI2HuScz1FC",
                CreatedDate = openedOn
            },
            new Member
            {
                Id = 3,
                Username = "priya",
                FirstName = "Priya",
                LastName = "Raman",
                Email = "priya.raman@speckitdemo.local",
                Address = "7 Lantern Street, Old Quarry",
                PasswordHash = "$2a$12$s8bbwMvHJ/GZoXoz5Crr4OGUsnH6/Brb0d9HSsYApoVyt5DPS./GK",
                CreatedDate = openedOn
            });
    }
}
