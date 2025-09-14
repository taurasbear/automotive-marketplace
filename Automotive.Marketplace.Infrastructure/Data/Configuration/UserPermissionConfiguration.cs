using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasOne(userPermission => userPermission.User)
            .WithMany(user => user.UserPermissions)
            .HasForeignKey(userPermission => userPermission.UserId);

        builder.Property(userPermission => userPermission.Permission)
            .HasConversion(
                permissionEnum => permissionEnum.ToString(),
                permissionString => (Permission)Enum.Parse(typeof(Permission), permissionString)
            );
    }
}
