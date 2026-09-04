using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260904183000_GoogleMemberSessionAndInvites")]
    public partial class GoogleMemberSessionAndInvites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Member",
                type: "varchar(320)",
                unicode: false,
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleSubject",
                table: "Member",
                type: "varchar(128)",
                unicode: false,
                maxLength: 128,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Member",
                type: "varchar(500)",
                unicode: false,
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Member_Email",
                table: "Member",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Member_GoogleSubject",
                table: "Member",
                column: "GoogleSubject");

            migrationBuilder.AddColumn<int>(
                name: "OwnerMemberId",
                table: "Band",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriveFolderId",
                table: "Band",
                type: "varchar(128)",
                unicode: false,
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriveFolderName",
                table: "Band",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Band_OwnerMemberId",
                table: "Band",
                column: "OwnerMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Band_OwnerMember",
                table: "Band",
                column: "OwnerMemberId",
                principalTable: "Member",
                principalColumn: "Id");

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "BandMember",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "member");

            migrationBuilder.Sql("DELETE FROM [GoogleAccount]");

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "GoogleAccount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAccount_MemberId",
                table: "GoogleAccount",
                column: "MemberId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GoogleAccount_Member",
                table: "GoogleAccount",
                column: "MemberId",
                principalTable: "Member",
                principalColumn: "Id");

            migrationBuilder.CreateTable(
                name: "BandInvite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BandId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "varchar(320)", unicode: false, maxLength: 320, nullable: false),
                    Code = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    RoleName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BandInvite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BandInvite_Band",
                        column: x => x.BandId,
                        principalTable: "Band",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BandInvite_Member",
                        column: x => x.CreatedBy,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BandInvite_Code",
                table: "BandInvite",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BandInvite_BandId",
                table: "BandInvite",
                column: "BandId");

            migrationBuilder.CreateIndex(
                name: "IX_BandInvite_CreatedBy",
                table: "BandInvite",
                column: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BandInvite");

            migrationBuilder.DropForeignKey(name: "FK_GoogleAccount_Member", table: "GoogleAccount");
            migrationBuilder.DropIndex(name: "IX_GoogleAccount_MemberId", table: "GoogleAccount");
            migrationBuilder.DropColumn(name: "MemberId", table: "GoogleAccount");

            migrationBuilder.DropColumn(name: "RoleName", table: "BandMember");

            migrationBuilder.DropForeignKey(name: "FK_Band_OwnerMember", table: "Band");
            migrationBuilder.DropIndex(name: "IX_Band_OwnerMemberId", table: "Band");
            migrationBuilder.DropColumn(name: "OwnerMemberId", table: "Band");
            migrationBuilder.DropColumn(name: "DriveFolderId", table: "Band");
            migrationBuilder.DropColumn(name: "DriveFolderName", table: "Band");

            migrationBuilder.DropIndex(name: "IX_Member_Email", table: "Member");
            migrationBuilder.DropIndex(name: "IX_Member_GoogleSubject", table: "Member");
            migrationBuilder.DropColumn(name: "Email", table: "Member");
            migrationBuilder.DropColumn(name: "GoogleSubject", table: "Member");

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Member",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldUnicode: false,
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
