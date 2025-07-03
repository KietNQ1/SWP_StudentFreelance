using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFreelance.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectCompletionConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BusinessConfirmedCompletion",
                table: "StudentApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "InterviewDate",
                table: "StudentApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "StudentApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StudentApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResumeLink",
                table: "StudentApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "StudentConfirmedCompletion",
                table: "StudentApplications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessConfirmedCompletion",
                table: "StudentApplications");

            migrationBuilder.DropColumn(
                name: "InterviewDate",
                table: "StudentApplications");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "StudentApplications");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StudentApplications");

            migrationBuilder.DropColumn(
                name: "ResumeLink",
                table: "StudentApplications");

            migrationBuilder.DropColumn(
                name: "StudentConfirmedCompletion",
                table: "StudentApplications");
        }
    }
}
