using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Seeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Companies (name) VALUES ('RG_Electronics')");
            migrationBuilder.Sql("INSERT INTO Companies (name) VALUES ('RG_Softwares')");
            migrationBuilder.Sql("INSERT INTO Employees (name, hours, companyId, imageUrl) VALUES ('Rafael', 0, 1, 'https://link')");
            migrationBuilder.Sql("INSERT INTO Employees (name, hours, companyId, imageUrl) VALUES ('Alex', 0, 1, 'https://link')");
            migrationBuilder.Sql("INSERT INTO Employees (name, hours, companyId, imageUrl) VALUES ('Bob', 0, 2, 'https://link')");
            migrationBuilder.Sql("INSERT INTO Employees (name, hours, companyId, imageUrl) VALUES ('John', 0, 2, 'https://link')");
            migrationBuilder.Sql("INSERT INTO Employees (name, hours, companyId, imageUrl) VALUES ('Kelvin', 0, 2, 'https://link')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE * FROM Companies"); 
            migrationBuilder.Sql("DELETE * FROM Employees"); 
                
        }
    }
}
