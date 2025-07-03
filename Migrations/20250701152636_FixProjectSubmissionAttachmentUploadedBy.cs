using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFreelance.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectSubmissionAttachmentUploadedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing foreign key constraint if it exists
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy') " +
                               "ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy]");

            // Add the foreign key back with correct delete behavior
            migrationBuilder.Sql("ALTER TABLE [ProjectSubmissionAttachments] " +
                               "ADD CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy] " +
                               "FOREIGN KEY ([UploadedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore original foreign key with cascade delete
            migrationBuilder.Sql("IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy') " +
                               "ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy]");

            migrationBuilder.Sql("ALTER TABLE [ProjectSubmissionAttachments] " +
                               "ADD CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy] " +
                               "FOREIGN KEY ([UploadedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE");
        }
    }
}
