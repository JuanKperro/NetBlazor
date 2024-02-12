using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgapeaBlazor2024.Server.Migrations
{
    /// <inheritdoc />
    public partial class _2_jk_creacionPedidoPayPal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PedidosPayPal",
                columns: table => new
                {
                    idPedidoPaypal = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    idCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idPedido = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidosPayPal", x => x.idPedidoPaypal);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidosPayPal");
        }
    }
}
