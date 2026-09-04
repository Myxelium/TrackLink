using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using api.Data;

#nullable disable

namespace api.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260904190700_AlbumDesk")]
    public partial class AlbumDesk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Album",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BandId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Archived = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalRule = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "all"),
                    ArtDriveFileId = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Album", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Album_Band",
                        column: x => x.BandId,
                        principalTable: "Band",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Album_Member",
                        column: x => x.CreatedBy,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlbumProposal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    SongId = table.Column<int>(type: "int", nullable: false),
                    ProposedBy = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumProposal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumProposal_Album",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumProposal_Song",
                        column: x => x.SongId,
                        principalTable: "Song",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumProposal_Member",
                        column: x => x.ProposedBy,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlbumTrack",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlbumId = table.Column<int>(type: "int", nullable: false),
                    SongId = table.Column<int>(type: "int", nullable: false),
                    ProposalId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTrack", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Album",
                        column: x => x.AlbumId,
                        principalTable: "Album",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Song",
                        column: x => x.SongId,
                        principalTable: "Song",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumTrack_Proposal",
                        column: x => x.ProposalId,
                        principalTable: "AlbumProposal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlbumProposalDecision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    Decision = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumProposalDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumProposalDecision_Proposal",
                        column: x => x.ProposalId,
                        principalTable: "AlbumProposal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlbumProposalDecision_Member",
                        column: x => x.MemberId,
                        principalTable: "Member",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(name: "IX_Album_BandId", table: "Album", column: "BandId");
            migrationBuilder.CreateIndex(name: "IX_Album_CreatedBy", table: "Album", column: "CreatedBy");
            migrationBuilder.CreateIndex(name: "IX_AlbumProposal_AlbumId", table: "AlbumProposal", column: "AlbumId");
            migrationBuilder.CreateIndex(name: "IX_AlbumProposal_SongId", table: "AlbumProposal", column: "SongId");
            migrationBuilder.CreateIndex(name: "IX_AlbumProposal_ProposedBy", table: "AlbumProposal", column: "ProposedBy");
            migrationBuilder.CreateIndex(
                name: "IX_AlbumTrack_AlbumId_SongId",
                table: "AlbumTrack",
                columns: new[] { "AlbumId", "SongId" },
                unique: true);
            migrationBuilder.CreateIndex(name: "IX_AlbumTrack_SongId", table: "AlbumTrack", column: "SongId");
            migrationBuilder.CreateIndex(name: "IX_AlbumTrack_ProposalId", table: "AlbumTrack", column: "ProposalId");
            migrationBuilder.CreateIndex(
                name: "IX_AlbumProposalDecision_ProposalId_MemberId",
                table: "AlbumProposalDecision",
                columns: new[] { "ProposalId", "MemberId" },
                unique: true);
            migrationBuilder.CreateIndex(name: "IX_AlbumProposalDecision_MemberId", table: "AlbumProposalDecision", column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AlbumProposalDecision");
            migrationBuilder.DropTable(name: "AlbumTrack");
            migrationBuilder.DropTable(name: "AlbumProposal");
            migrationBuilder.DropTable(name: "Album");
        }
    }
}
