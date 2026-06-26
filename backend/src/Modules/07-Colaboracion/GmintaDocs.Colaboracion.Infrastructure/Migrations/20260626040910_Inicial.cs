using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.Colaboracion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comentarios",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    autor = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    id_noticia = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "noticias",
                columns: table => new
                {
                    id_noticia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    autor = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_noticias", x => x.id_noticia);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    remitente = table.Column<string>(type: "text", nullable: false),
                    para = table.Column<string>(type: "text", nullable: false),
                    copia = table.Column<string>(type: "text", nullable: true),
                    copia_oculta = table.Column<string>(type: "text", nullable: true),
                    asunto = table.Column<string>(type: "text", nullable: false),
                    es_html = table.Column<bool>(type: "boolean", nullable: false),
                    cuerpo = table.Column<string>(type: "text", nullable: false),
                    adjuntos = table.Column<string>(type: "text", nullable: true),
                    enviado = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comentarios");

            migrationBuilder.DropTable(
                name: "noticias");

            migrationBuilder.DropTable(
                name: "notificaciones");
        }
    }
}
