using Microsoft.EntityFrameworkCore.Migrations;

namespace LibraryManagementSystem.Migrations
{
    public partial class PopulateGenres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO Genres (Id, Name) VALUES ('1203dbe0-cb52-487b-8a02-623fd6edabfc', 'Action and Adventure')");
            migrationBuilder.Sql($"INSERT INTO Genres (Id, Name) VALUES ('6756fb2c-dcd6-4c48-bd65-42e26db83294', 'Comic Book or Graphic Novel')");
            migrationBuilder.Sql($"INSERT INTO Genres (Id, Name) VALUES ('540e1346-9ae0-4cf6-a41b-c586a1fa04cc', 'Suspense and Thrillers')");
            migrationBuilder.Sql($"INSERT INTO Genres (Id, Name) VALUES ('abaed68f-22fe-4f25-9709-f3f16f5ec2dc', 'Romance – Contemporary and Historical')");
            migrationBuilder.Sql($"INSERT INTO Genres (Id, Name) VALUES ('1b9e7fcd-9906-4be2-874e-69215b004cd2', 'Religious and Self-help')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
