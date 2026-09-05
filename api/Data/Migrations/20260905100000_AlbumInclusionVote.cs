using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260905100000_AlbumInclusionVote")]
    public partial class AlbumInclusionVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlbumId",
                table: "Vote",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Choice",
                table: "Vote",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vote_AlbumId_MemberId_SongId",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId", "SongId" },
                unique: true,
                filter: "[AlbumId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Album",
                table: "Vote",
                column: "AlbumId",
                principalTable: "Album",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Album",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_AlbumId_MemberId_SongId",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "Choice",
                table: "Vote");
        }
    }
}
