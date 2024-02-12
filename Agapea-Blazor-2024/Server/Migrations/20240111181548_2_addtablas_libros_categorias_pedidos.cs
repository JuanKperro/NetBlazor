using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgapeaBlazor2024.Server.Migrations
{
    /// <inheritdoc />
    public partial class _2_addtablas_libros_categorias_pedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NIF",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "IdCliente",
                table: "Direcciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");



            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    IdPedido = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "datetime2", nullable: false),
                    listaItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DireccionEnvioIdDireccion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DireccionFacturacionIdDireccion = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    GastosEnvio = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    Total = table.Column<decimal>(type: "DECIMAL(5,2)", nullable: false),
                    EstadoPedido = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.IdPedido);
                    table.ForeignKey(
                        name: "FK_Pedidos_Direcciones_DireccionEnvioIdDireccion",
                        column: x => x.DireccionEnvioIdDireccion,
                        principalTable: "Direcciones",
                        principalColumn: "IdDireccion",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Pedidos_Direcciones_DireccionFacturacionIdDireccion",
                        column: x => x.DireccionFacturacionIdDireccion,
                        principalTable: "Direcciones",
                        principalColumn: "IdDireccion",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DireccionEnvioIdDireccion",
                table: "Pedidos",
                column: "DireccionEnvioIdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_DireccionFacturacionIdDireccion",
                table: "Pedidos",
                column: "DireccionFacturacionIdDireccion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Libros");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropColumn(
                name: "IdCliente",
                table: "Direcciones");

            migrationBuilder.AddColumn<string>(
                name: "NIF",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
