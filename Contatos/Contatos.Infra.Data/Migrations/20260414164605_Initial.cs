using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contatos.Infra.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Email",
                table: "Usuario",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            // --- INSERÇÃO DE 10 USUÁRIOS (SEED DATA) ---
            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "Id", "Nome", "Email", "Ativo", "DataCadastro" },
                values: new object[,]
                {
                    { Guid.NewGuid(), "Joaquim Venâncio", "joaquim.venancio@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Carlos Chagas", "carlos.chagas@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Oswaldo Cruz", "oswaldo.cruz@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Ana Nery", "ana.nery@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Vital Brazil", "vital.brazil@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Ruth Sontag", "ruth.sontag@provedor.com", false, DateTime.Now },
                    { Guid.NewGuid(), "Adriana Melo", "adriana.melo@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Zilda Arns", "zilda.arns@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Ennio Candotti", "ennio.candotti@provedor.com", true, DateTime.Now },
                    { Guid.NewGuid(), "Marcelo Gleiser", "marcelo.gleiser@provedor.com", true, DateTime.Now }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}