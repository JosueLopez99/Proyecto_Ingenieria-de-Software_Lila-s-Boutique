using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp1.Migrations
{
    /// <inheritdoc />
    public partial class SeedInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "IdRol", "NivelPrivilegio", "NombreRol" },
                values: new object[] { 1, 1, "Administradora" });

            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "IdUsuario", "Correo", "EstadoActivo", "HashCredencial", "IdRol" },
                values: new object[] { 1, "admin@lilasboutique.com", true, "$2a$11$LOQ89WtwUXYiiS8So59bhuJo5509W8QJurM0eMoB06G2wylr17oiu", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoAuditoria_IdUsuario",
                table: "MovimientoAuditoria",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientoAuditoria_IdVariante",
                table: "MovimientoAuditoria",
                column: "IdVariante");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdRol",
                table: "Usuario",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_VariantePrenda_CodigoPrenda",
                table: "VariantePrenda",
                column: "CodigoPrenda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientoAuditoria");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "VariantePrenda");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Prenda");
        }
    }
}
