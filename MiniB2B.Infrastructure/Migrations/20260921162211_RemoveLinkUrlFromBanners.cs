using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniB2B.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLinkUrlFromBanners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "Banners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "Banners",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
