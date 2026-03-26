using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V1_SubmitterDeclarationAndRemarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SubmitterDeclarationAccepted",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterDeclarationText_Alt",
                table: "Proposals",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterDeclarationText_En",
                table: "Proposals",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterRemarks_Alt",
                table: "Proposals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmitterRemarks_En",
                table: "Proposals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmitterDeclarationAccepted",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SubmitterDeclarationText_Alt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SubmitterDeclarationText_En",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SubmitterRemarks_Alt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SubmitterRemarks_En",
                table: "Proposals");
        }
    }
}
