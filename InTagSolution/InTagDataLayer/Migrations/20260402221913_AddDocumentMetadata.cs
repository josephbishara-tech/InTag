using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InTagDataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentLink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: false),
                    TargetDocumentId = table.Column<int>(type: "int", nullable: false),
                    LinkType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentLink_Document_SourceDocumentId",
                        column: x => x.SourceDocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentLink_Document_TargetDocumentId",
                        column: x => x.TargetDocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    UserFileId = table.Column<int>(type: "int", nullable: true),
                    UserFolderId = table.Column<int>(type: "int", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTag_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentTag_UserFile_UserFileId",
                        column: x => x.UserFileId,
                        principalTable: "UserFile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentTag_UserFolder_UserFolderId",
                        column: x => x.UserFolderId,
                        principalTable: "UserFolder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MetadataTemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApplicableDocType = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetadataFieldDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayLabel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FieldType = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Placeholder = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    HelpText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataFieldDefinition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetadataFieldDefinition_MetadataTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "MetadataTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: true),
                    UserFileId = table.Column<int>(type: "int", nullable: true),
                    UserFolderId = table.Column<int>(type: "int", nullable: true),
                    FieldDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentMetadata_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentMetadata_MetadataFieldDefinition_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "MetadataFieldDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentMetadata_UserFile_UserFileId",
                        column: x => x.UserFileId,
                        principalTable: "UserFile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentMetadata_UserFolder_UserFolderId",
                        column: x => x.UserFolderId,
                        principalTable: "UserFolder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocLink_Src_Tgt_Type",
                table: "DocumentLink",
                columns: new[] { "SourceDocumentId", "TargetDocumentId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLink_IsActive",
                table: "DocumentLink",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLink_TargetDocumentId",
                table: "DocumentLink",
                column: "TargetDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLink_TenantId_IsActive",
                table: "DocumentLink",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DocMeta_DocId_FieldId",
                table: "DocumentMetadata",
                columns: new[] { "DocumentId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocMeta_FileId_FieldId",
                table: "DocumentMetadata",
                columns: new[] { "UserFileId", "FieldDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_FieldDefinitionId",
                table: "DocumentMetadata",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_IsActive",
                table: "DocumentMetadata",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_TenantId_IsActive",
                table: "DocumentMetadata",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_UserFolderId",
                table: "DocumentMetadata",
                column: "UserFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_DocTag_DocId_Tag",
                table: "DocumentTag",
                columns: new[] { "DocumentId", "Tag" });

            migrationBuilder.CreateIndex(
                name: "IX_DocTag_Tag",
                table: "DocumentTag",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTag_IsActive",
                table: "DocumentTag",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTag_TenantId_IsActive",
                table: "DocumentTag",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTag_UserFileId",
                table: "DocumentTag",
                column: "UserFileId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTag_UserFolderId",
                table: "DocumentTag",
                column: "UserFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataFieldDefinition_IsActive",
                table: "MetadataFieldDefinition",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataFieldDefinition_TenantId_IsActive",
                table: "MetadataFieldDefinition",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MetaField_Template_Sort",
                table: "MetadataFieldDefinition",
                columns: new[] { "TemplateId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MetadataTemplate_IsActive",
                table: "MetadataTemplate",
                column: "IsActive",
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataTemplate_TenantId_IsActive",
                table: "MetadataTemplate",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MetaTemplate_TenantName",
                table: "MetadataTemplate",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentLink");

            migrationBuilder.DropTable(
                name: "DocumentMetadata");

            migrationBuilder.DropTable(
                name: "DocumentTag");

            migrationBuilder.DropTable(
                name: "MetadataFieldDefinition");

            migrationBuilder.DropTable(
                name: "MetadataTemplate");
        }
    }
}
