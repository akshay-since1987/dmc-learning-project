using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V1_FundTypeOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FundOwner",
                table: "Proposals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "FundTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsCentral",
                table: "FundTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDpdc",
                table: "FundTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMnp",
                table: "FundTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsState",
                table: "FundTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FundOwner",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "IsCentral",
                table: "FundTypes");

            migrationBuilder.DropColumn(
                name: "IsDpdc",
                table: "FundTypes");

            migrationBuilder.DropColumn(
                name: "IsMnp",
                table: "FundTypes");

            migrationBuilder.DropColumn(
                name: "IsState",
                table: "FundTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "FundTypes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
