using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.Organizacion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id_empresa = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    razon_social = table.Column<string>(type: "text", nullable: false),
                    nit = table.Column<string>(type: "text", nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    ciudad = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    notas = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "feriados",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fecha_feriado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feriados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros",
                columns: table => new
                {
                    id_parametro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    grupo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros", x => x.id_parametro);
                });

            migrationBuilder.CreateTable(
                name: "radicados_empresa",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_empresa = table.Column<long>(type: "bigint", nullable: false),
                    radicado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_radicados_empresa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "radicados_sucursal",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_sucursal = table.Column<long>(type: "bigint", nullable: false),
                    radicado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_radicados_sucursal", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles_empresa",
                columns: table => new
                {
                    id_empresa = table.Column<long>(type: "bigint", nullable: false),
                    id_rol = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles_empresa", x => new { x.id_empresa, x.id_rol });
                });

            migrationBuilder.CreateTable(
                name: "sucursales",
                columns: table => new
                {
                    id_sucursal = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_empresa = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: false),
                    telefono = table.Column<string>(type: "text", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.id_sucursal);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropTable(
                name: "feriados");

            migrationBuilder.DropTable(
                name: "parametros");

            migrationBuilder.DropTable(
                name: "radicados_empresa");

            migrationBuilder.DropTable(
                name: "radicados_sucursal");

            migrationBuilder.DropTable(
                name: "roles_empresa");

            migrationBuilder.DropTable(
                name: "sucursales");
        }
    }
}
