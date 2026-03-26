using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProposalManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V1_WizardSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccountantWillingToProcess",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AccountingNumber",
                table: "Proposals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAmount",
                table: "Proposals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedStep",
                table: "Proposals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FirstApproverRole",
                table: "Proposals",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPreviousExpenditure",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HomeId",
                table: "Proposals",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LegalSurveyDone",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "PreviousExpenditureAmount",
                table: "Proposals",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicationDays",
                table: "Proposals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason_Alt",
                table: "Proposals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason_En",
                table: "Proposals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SameWorkProposedInOtherFund",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VendorTenureCompleted",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WorkPlaceWithinPalika",
                table: "Proposals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InAppNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title_En = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Title_Alt = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Message_En = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Message_Alt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InAppNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProposalSignatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageHistoryId = table.Column<long>(type: "bigint", nullable: false),
                    SignedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    PositionX = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    PositionY = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    Rotation = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    GeneratedPdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalSignatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalSignatures_ProposalStageHistory_StageHistoryId",
                        column: x => x.StageHistoryId,
                        principalTable: "ProposalStageHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposalSignatures_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposalSignatures_Users_SignedById",
                        column: x => x.SignedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProposalStepLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepNumber = table.Column<int>(type: "int", nullable: false),
                    LockedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReleased = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProposalStepLocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProposalStepLocks_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProposalStepLocks_Users_LockedById",
                        column: x => x.LockedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_CreatedAt",
                table: "InAppNotifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_UserId_IsRead",
                table: "InAppNotifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_ProposalSignatures_ProposalId",
                table: "ProposalSignatures",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalSignatures_SignedById",
                table: "ProposalSignatures",
                column: "SignedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalSignatures_StageHistoryId",
                table: "ProposalSignatures",
                column: "StageHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStepLocks_ExpiresAt",
                table: "ProposalStepLocks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStepLocks_LockedById",
                table: "ProposalStepLocks",
                column: "LockedById");

            migrationBuilder.CreateIndex(
                name: "IX_ProposalStepLocks_ProposalId_StepNumber",
                table: "ProposalStepLocks",
                columns: new[] { "ProposalId", "StepNumber" },
                filter: "[IsReleased] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InAppNotifications");

            migrationBuilder.DropTable(
                name: "ProposalSignatures");

            migrationBuilder.DropTable(
                name: "ProposalStepLocks");

            migrationBuilder.DropColumn(
                name: "AccountantWillingToProcess",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "AccountingNumber",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "BalanceAmount",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "CompletedStep",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "FirstApproverRole",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "HasPreviousExpenditure",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "HomeId",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "LegalSurveyDone",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "PreviousExpenditureAmount",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "PublicationDays",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Reason_Alt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "Reason_En",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "SameWorkProposedInOtherFund",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "VendorTenureCompleted",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "WorkPlaceWithinPalika",
                table: "Proposals");
        }
    }
}
