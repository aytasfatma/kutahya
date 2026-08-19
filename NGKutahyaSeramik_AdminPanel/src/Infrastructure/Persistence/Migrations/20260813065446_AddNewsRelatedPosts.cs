using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsRelatedPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsRelatedPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsId = table.Column<int>(type: "int", nullable: false),
                    RelatedNewsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsRelatedPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsRelatedPosts_News_NewsId",
                        column: x => x.NewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsRelatedPosts_News_RelatedNewsId",
                        column: x => x.RelatedNewsId,
                        principalTable: "News",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsRelatedPosts_NewsId_RelatedNewsId",
                table: "NewsRelatedPosts",
                columns: new[] { "NewsId", "RelatedNewsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsRelatedPosts_RelatedNewsId",
                table: "NewsRelatedPosts",
                column: "RelatedNewsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsRelatedPosts");
        }
    }
}
