using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgapeaBlazor2024.Server.Migrations
{
    /// <inheritdoc />
    public partial class _03_jk_tablaOpiniones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Opiniones",
                columns: table => new
                {
                    IdOpinion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdLibro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Libro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TextoOpinion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Puntuacion = table.Column<int>(type: "int", nullable: false),
                    FechaOpinion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opiniones", x => x.IdOpinion);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Opiniones");
        }
    }
}
