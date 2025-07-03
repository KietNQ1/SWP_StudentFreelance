using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFreelance.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectSubmissionAttachmentConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verificar si existen los indices para la tabla ProjectSubmissionAttachments
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID')
                    ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy')
                    ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy];
            ");

            // Recrear los indices con las relaciones correctas
            migrationBuilder.Sql(@"
                ALTER TABLE [ProjectSubmissionAttachments] 
                ADD CONSTRAINT [FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID] 
                FOREIGN KEY ([SubmissionID]) REFERENCES [ProjectSubmissions] ([SubmissionID]) ON DELETE CASCADE;

                ALTER TABLE [ProjectSubmissionAttachments] 
                ADD CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy] 
                FOREIGN KEY ([UploadedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID')
                    ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID];
                
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy')
                    ALTER TABLE [ProjectSubmissionAttachments] DROP CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy];
            ");

            // Restore previous constraints
            migrationBuilder.Sql(@"
                ALTER TABLE [ProjectSubmissionAttachments] 
                ADD CONSTRAINT [FK_ProjectSubmissionAttachments_ProjectSubmissions_SubmissionID] 
                FOREIGN KEY ([SubmissionID]) REFERENCES [ProjectSubmissions] ([SubmissionID]) ON DELETE RESTRICT;

                ALTER TABLE [ProjectSubmissionAttachments] 
                ADD CONSTRAINT [FK_ProjectSubmissionAttachments_AspNetUsers_UploadedBy] 
                FOREIGN KEY ([UploadedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE RESTRICT;
            ");
        }
    }
}
