using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V1_AccountingOfficer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccountingOfficerId",
                table: "Proposals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proposals_AccountingOfficerId",
                table: "Proposals",
                column: "AccountingOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Users_AccountingOfficerId",
                table: "Proposals",
                column: "AccountingOfficerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Users_AccountingOfficerId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_Proposals_AccountingOfficerId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "AccountingOfficerId",
                table: "Proposals");
        }
    }
}
