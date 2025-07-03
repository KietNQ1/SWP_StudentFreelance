using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFreelance.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSubmissionRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy",
                table: "ProjectSubmissionAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID",
                table: "ProjectSubmissionAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_ApplicationID",
                table: "ProjectSubmissions");

            migrationBuilder.AddColumn<int>(
                name: "StudentApplicationApplicationID",
                table: "ProjectSubmissions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSubmissions_StudentApplicationApplicationID",
                table: "ProjectSubmissions",
                column: "StudentApplicationApplicationID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy",
                table: "ProjectSubmissionAttachments",
                column: "UploadedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID",
                table: "ProjectSubmissionAttachments",
                column: "SubmissionID",
                principalTable: "ProjectSubmissions",
                principalColumn: "SubmissionID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_ApplicationID",
                table: "ProjectSubmissions",
                column: "ApplicationID",
                principalTable: "StudentApplications",
                principalColumn: "ApplicationID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_StudentApplicationApplicationID",
                table: "ProjectSubmissions",
                column: "StudentApplicationApplicationID",
                principalTable: "StudentApplications",
                principalColumn: "ApplicationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy",
                table: "ProjectSubmissionAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID",
                table: "ProjectSubmissionAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_ApplicationID",
                table: "ProjectSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_StudentApplicationApplicationID",
                table: "ProjectSubmissions");

            migrationBuilder.DropIndex(
                name: "IX_ProjectSubmissions_StudentApplicationApplicationID",
                table: "ProjectSubmissions");

            migrationBuilder.DropColumn(
                name: "StudentApplicationApplicationID",
                table: "ProjectSubmissions");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy",
                table: "ProjectSubmissionAttachments",
                column: "UploadedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID",
                table: "ProjectSubmissionAttachments",
                column: "SubmissionID",
                principalTable: "ProjectSubmissions",
                principalColumn: "SubmissionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectSubmissions_StudentApplications_ApplicationID",
                table: "ProjectSubmissions",
                column: "ApplicationID",
                principalTable: "StudentApplications",
                principalColumn: "ApplicationID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
