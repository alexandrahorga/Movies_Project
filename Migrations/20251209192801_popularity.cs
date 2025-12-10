using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies_Project.Migrations
{
    /// <inheritdoc />
    public partial class popularity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Popularity",
                table: "Movie",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Popularity",
                table: "Movie");
        }
    }
}
