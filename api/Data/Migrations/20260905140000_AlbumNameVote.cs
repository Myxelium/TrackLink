using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260905140000_AlbumNameVote")]
    public partial class AlbumNameVote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Song",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_AlbumId_MemberId_SongId",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.AlterColumn<int>(
                name: "SongId",
                table: "Vote",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "Vote",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Vote",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Vote SET Kind = 'inclusion' WHERE AlbumId IS NOT NULL AND Kind IS NULL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_Inclusion",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId", "SongId" },
                unique: true,
                filter: "[Kind] = 'inclusion' AND [AlbumId] IS NOT NULL AND [SongId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_AlbumName",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId" },
                unique: true,
                filter: "[Kind] = 'album_name'");

            migrationBuilder.CreateIndex(
                name: "IX_Vote_SongName",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId", "SongId" },
                unique: true,
                filter: "[Kind] = 'song_name' AND [SongId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Song",
                table: "Vote",
                column: "SongId",
                principalTable: "Song",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vote_Song",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_Inclusion",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_AlbumName",
                table: "Vote");

            migrationBuilder.DropIndex(
                name: "IX_Vote_SongName",
                table: "Vote");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vote",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "Vote");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Vote");

            migrationBuilder.AlterColumn<int>(
                name: "SongId",
                table: "Vote",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vote",
                table: "Vote",
                columns: new[] { "Id", "MemberId", "SongId" });

            migrationBuilder.CreateIndex(
                name: "IX_Vote_AlbumId_MemberId_SongId",
                table: "Vote",
                columns: new[] { "AlbumId", "MemberId", "SongId" },
                unique: true,
                filter: "[AlbumId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vote_Song",
                table: "Vote",
                column: "SongId",
                principalTable: "Song",
                principalColumn: "Id");
        }
    }
}
