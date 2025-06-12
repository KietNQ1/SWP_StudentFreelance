using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFreelance.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsWithEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ProficiencyLevel",
                table: "StudentSkills");

            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ImportanceLevel",
                table: "ProjectSkillsRequired");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "WorkType",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NotificationType",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "Avatar",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeID",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProficiencyLevelID",
                table: "StudentSkills",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeID",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ImportanceLevelID",
                table: "ProjectSkillsRequired",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Projects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "StatusID",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeID",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypeID",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccountStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "ImportanceLevels",
                columns: table => new
                {
                    LevelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportanceLevels", x => x.LevelID);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "ProficiencyLevels",
                columns: table => new
                {
                    LevelID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LevelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProficiencyLevels", x => x.LevelID);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTypes", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "ReportStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "ReportTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTypes", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatuses",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.TypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatusID",
                table: "Users",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StatusID",
                table: "Transactions",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TypeID",
                table: "Transactions",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentSkills_ProficiencyLevelID",
                table: "StudentSkills",
                column: "ProficiencyLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StatusID",
                table: "Reports",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TypeID",
                table: "Reports",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSkillsRequired_ImportanceLevelID",
                table: "ProjectSkillsRequired",
                column: "ImportanceLevelID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StatusID",
                table: "Projects",
                column: "StatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TypeID",
                table: "Projects",
                column: "TypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TypeID",
                table: "Notifications",
                column: "TypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_NotificationTypes_TypeID",
                table: "Notifications",
                column: "TypeID",
                principalTable: "NotificationTypes",
                principalColumn: "TypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectStatuses_StatusID",
                table: "Projects",
                column: "StatusID",
                principalTable: "ProjectStatuses",
                principalColumn: "StatusID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ProjectTypes_TypeID",
                table: "Projects",
                column: "TypeID",
                principalTable: "ProjectTypes",
                principalColumn: "TypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSkillsRequired_ImportanceLevels_ImportanceLevelID",
                table: "ProjectSkillsRequired",
                column: "ImportanceLevelID",
                principalTable: "ImportanceLevels",
                principalColumn: "LevelID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportStatuses_StatusID",
                table: "Reports",
                column: "StatusID",
                principalTable: "ReportStatuses",
                principalColumn: "StatusID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ReportTypes_TypeID",
                table: "Reports",
                column: "TypeID",
                principalTable: "ReportTypes",
                principalColumn: "TypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentSkills_ProficiencyLevels_ProficiencyLevelID",
                table: "StudentSkills",
                column: "ProficiencyLevelID",
                principalTable: "ProficiencyLevels",
                principalColumn: "LevelID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TransactionStatuses_StatusID",
                table: "Transactions",
                column: "StatusID",
                principalTable: "TransactionStatuses",
                principalColumn: "StatusID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TransactionTypes_TypeID",
                table: "Transactions",
                column: "TypeID",
                principalTable: "TransactionTypes",
                principalColumn: "TypeID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_AccountStatuses_StatusID",
                table: "Users",
                column: "StatusID",
                principalTable: "AccountStatuses",
                principalColumn: "StatusID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_NotificationTypes_TypeID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectStatuses_StatusID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ProjectTypes_TypeID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSkillsRequired_ImportanceLevels_ImportanceLevelID",
                table: "ProjectSkillsRequired");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportStatuses_StatusID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ReportTypes_TypeID",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentSkills_ProficiencyLevels_ProficiencyLevelID",
                table: "StudentSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TransactionStatuses_StatusID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TransactionTypes_TypeID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_AccountStatuses_StatusID",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AccountStatuses");

            migrationBuilder.DropTable(
                name: "ImportanceLevels");

            migrationBuilder.DropTable(
                name: "NotificationTypes");

            migrationBuilder.DropTable(
                name: "ProficiencyLevels");

            migrationBuilder.DropTable(
                name: "ProjectStatuses");

            migrationBuilder.DropTable(
                name: "ProjectTypes");

            migrationBuilder.DropTable(
                name: "ReportStatuses");

            migrationBuilder.DropTable(
                name: "ReportTypes");

            migrationBuilder.DropTable(
                name: "TransactionStatuses");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Users_StatusID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_StatusID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TypeID",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_StudentSkills_ProficiencyLevelID",
                table: "StudentSkills");

            migrationBuilder.DropIndex(
                name: "IX_Reports_StatusID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_TypeID",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_ProjectSkillsRequired_ImportanceLevelID",
                table: "ProjectSkillsRequired");

            migrationBuilder.DropIndex(
                name: "IX_Projects_StatusID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TypeID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_TypeID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Avatar",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TypeID",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ProficiencyLevelID",
                table: "StudentSkills");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "TypeID",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ImportanceLevelID",
                table: "ProjectSkillsRequired");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "StatusID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TypeID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TypeID",
                table: "Notifications");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProficiencyLevel",
                table: "StudentSkills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImportanceLevel",
                table: "ProjectSkillsRequired",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkType",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NotificationType",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
