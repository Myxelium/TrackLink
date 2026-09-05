using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260904194500_ProposalReviews")]
    public partial class ProposalReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlbumProposalReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    StartMs = table.Column<int>(type: "int", nullable: false),
                    EndMs = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumProposalReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumProposalReview_Proposal",
                        column: x => x.ProposalId,
                        principalTable: "AlbumProposal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumProposalReview_Member",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumProposalReview_ProposalId",
                table: "AlbumProposalReview",
                column: "ProposalId");
            migrationBuilder.CreateIndex(
                name: "IX_AlbumProposalReview_MemberId",
                table: "AlbumProposalReview",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlbumProposalReview");
        }
    }
}
