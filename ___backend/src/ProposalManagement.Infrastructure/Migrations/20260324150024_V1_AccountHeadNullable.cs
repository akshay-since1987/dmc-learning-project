using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V1_AccountHeadNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_AccountHeads_AccountHeadId",
                table: "Proposals");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Proposals",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_AccountHeads_AccountHeadId",
                table: "Proposals",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_AccountHeads_AccountHeadId",
                table: "Proposals");

            migrationBuilder.AlterColumn<Guid>(
                name: "AccountHeadId",
                table: "Proposals",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_AccountHeads_AccountHeadId",
                table: "Proposals",
                column: "AccountHeadId",
                principalTable: "AccountHeads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
