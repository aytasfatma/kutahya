using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDealerCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Dealers] SET [Category] = 'SalesPoint' WHERE [Category] IN ('Dealer', 'Showroom');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Dealers] SET [Category] = 'Dealer' WHERE [Category] = 'SalesPoint';");
            migrationBuilder.Sql("UPDATE [Dealers] SET [Category] = 'Showroom' WHERE [Category] IN ('GeneralHeadquarters', 'Factory');");
        }
    }
}
