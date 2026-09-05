using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260905093200_SongVersionMeta")]
    public partial class SongVersionMeta : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentMd5",
                table: "Song",
                type: "varchar(32)",
                unicode: false,
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SourceModifiedAt",
                table: "Song",
                type: "datetime",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentMd5",
                table: "Song");

            migrationBuilder.DropColumn(
                name: "SourceModifiedAt",
                table: "Song");
        }
    }
}
