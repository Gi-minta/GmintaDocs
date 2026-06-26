using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.AdminFormularios.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campos",
                columns: table => new
                {
                    id_campo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    columna = table.Column<string>(type: "text", nullable: false),
                    tipo_dato = table.Column<short>(type: "smallint", nullable: false),
                    long_dato = table.Column<int>(type: "integer", nullable: false),
                    control = table.Column<short>(type: "smallint", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    unico = table.Column<bool>(type: "boolean", nullable: false),
                    mostrar = table.Column<bool>(type: "boolean", nullable: false),
                    mascara = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    requerido = table.Column<bool>(type: "boolean", nullable: false),
                    enlace = table.Column<short>(type: "smallint", nullable: true),
                    id_enlace = table.Column<long>(type: "bigint", nullable: true),
                    cascada_de = table.Column<string>(type: "text", nullable: true),
                    campo1 = table.Column<string>(type: "text", nullable: true),
                    campo2 = table.Column<string>(type: "text", nullable: true),
                    sticker = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campos", x => x.id_campo);
                });

            migrationBuilder.CreateTable(
                name: "copias",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_directorio = table.Column<long>(type: "bigint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_copias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "formularios",
                columns: table => new
                {
                    id_formulario = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    tabla = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    id_padre = table.Column<long>(type: "bigint", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    imagen = table.Column<bool>(type: "boolean", nullable: false),
                    long_radicado = table.Column<short>(type: "smallint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formularios", x => x.id_formulario);
                });

            migrationBuilder.CreateTable(
                name: "item_lista",
                columns: table => new
                {
                    id_item = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_lista = table.Column<long>(type: "bigint", nullable: false),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    id_item_padre = table.Column<long>(type: "bigint", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_lista", x => x.id_item);
                });

            migrationBuilder.CreateTable(
                name: "lista",
                columns: table => new
                {
                    id_lista = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    id_lista_padre = table.Column<long>(type: "bigint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lista", x => x.id_lista);
                });

            migrationBuilder.CreateTable(
                name: "mensajes_notificacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    mensaje = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<int>(type: "integer", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensajes_notificacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parametros_mensaje",
                columns: table => new
                {
                    id_parametro = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_plantilla = table.Column<int>(type: "integer", nullable: false),
                    parametro = table.Column<string>(type: "text", nullable: false),
                    tabla = table.Column<string>(type: "text", nullable: false),
                    campo = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametros_mensaje", x => x.id_parametro);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campos");

            migrationBuilder.DropTable(
                name: "copias");

            migrationBuilder.DropTable(
                name: "formularios");

            migrationBuilder.DropTable(
                name: "item_lista");

            migrationBuilder.DropTable(
                name: "lista");

            migrationBuilder.DropTable(
                name: "mensajes_notificacion");

            migrationBuilder.DropTable(
                name: "parametros_mensaje");
        }
    }
}
