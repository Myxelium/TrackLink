using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260905180000_AlbumOrderVote")]
    public partial class AlbumOrderVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OrderLocked",
                table: "Album",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Vote_Order",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId", "SongId" },
                unique: true,
                filter: "[Kind] = 'order' AND [AlbumId] IS NOT NULL AND [SongId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vote_Order",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "OrderLocked",
                table: "Album");
        }
    }
}
