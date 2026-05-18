using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

using Maliev.FacilityService.Infrastructure.Data;

#nullable disable

namespace Maliev.FacilityService.Infrastructure.Data.Migrations
{
    [DbContext(typeof(FacilityDbContext))]
    [Migration("20260518110000_AddMaintenanceLogDocuments")]
    partial class AddMaintenanceLogDocuments
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);
#pragma warning restore 612, 618
        }
    }
}
