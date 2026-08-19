using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogRelatedPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogRelatedPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogId = table.Column<int>(type: "int", nullable: false),
                    RelatedBlogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogRelatedPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlogRelatedPosts_Blogs_BlogId",
                        column: x => x.BlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogRelatedPosts_Blogs_RelatedBlogId",
                        column: x => x.RelatedBlogId,
                        principalTable: "Blogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogRelatedPosts_BlogId_RelatedBlogId",
                table: "BlogRelatedPosts",
                columns: new[] { "BlogId", "RelatedBlogId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlogRelatedPosts_RelatedBlogId",
                table: "BlogRelatedPosts",
                column: "RelatedBlogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogRelatedPosts");
        }
    }
}
