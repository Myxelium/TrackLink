using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260904120000_FixRelationsAndGoogleAccount")]
    public partial class FixRelationsAndGoogleAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BandMember_Band",
                table: "BandMember");

            migrationBuilder.DropForeignKey(
                name: "FK_MemberRole_MemberRole",
                table: "MemberRole");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BandMember",
                table: "BandMember",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BandMember_BandId",
                table: "BandMember",
                column: "BandId");

            migrationBuilder.AddForeignKey(
                name: "FK_BandMember_Band",
                table: "BandMember",
                column: "BandId",
                principalTable: "Band",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberRole_Role",
                table: "MemberRole",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Song",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_SongIdentifier_BandId",
                table: "SongIdentifier",
                column: "BandId");

            migrationBuilder.CreateIndex(
                name: "IX_SongIdentifier_SongId",
                table: "SongIdentifier",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_SongIdentifier_Band",
                table: "SongIdentifier",
                column: "BandId",
                principalTable: "Band",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SongIdentifier_Song",
                table: "SongIdentifier",
                column: "SongId",
                principalTable: "Song",
                principalColumn: "Id");

            migrationBuilder.CreateTable(
                name: "GoogleAccount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleAccount", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GoogleAccount");

            migrationBuilder.DropForeignKey(name: "FK_SongIdentifier_Band", table: "SongIdentifier");
            migrationBuilder.DropForeignKey(name: "FK_SongIdentifier_Song", table: "SongIdentifier");
            migrationBuilder.DropIndex(name: "IX_SongIdentifier_BandId", table: "SongIdentifier");
            migrationBuilder.DropIndex(name: "IX_SongIdentifier_SongId", table: "SongIdentifier");

            migrationBuilder.DropForeignKey(name: "FK_MemberRole_Role", table: "MemberRole");
            migrationBuilder.DropForeignKey(name: "FK_BandMember_Band", table: "BandMember");
            migrationBuilder.DropIndex(name: "IX_BandMember_BandId", table: "BandMember");
            migrationBuilder.DropPrimaryKey(name: "PK_BandMember", table: "BandMember");

            migrationBuilder.AddForeignKey(
                name: "FK_BandMember_Band",
                table: "BandMember",
                column: "MemberId",
                principalTable: "Band",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberRole_MemberRole",
                table: "MemberRole",
                column: "RoleId",
                principalTable: "MemberRole",
                principalColumn: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Song",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);
        }
    }
}
