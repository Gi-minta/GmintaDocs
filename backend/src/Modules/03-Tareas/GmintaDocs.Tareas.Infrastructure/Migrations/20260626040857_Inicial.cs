using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.Tareas.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agenda",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    titulo = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    comienza = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finaliza = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    es_todo_dia = table.Column<bool>(type: "boolean", nullable: true),
                    regla_recurrencia = table.Column<string>(type: "text", nullable: true),
                    excepcion_recurrencia = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agenda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "carpeta_tarea",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    padre = table.Column<short>(type: "smallint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carpeta_tarea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comentarios_tarea",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_tarea = table.Column<long>(type: "bigint", nullable: false),
                    id_workflow = table.Column<long>(type: "bigint", nullable: false),
                    autor = table.Column<string>(type: "text", nullable: false),
                    avatar = table.Column<string>(type: "text", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentarios_tarea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contenido_carpeta",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_tarea = table.Column<long>(type: "bigint", nullable: false),
                    id_carpeta_tarea = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contenido_carpeta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lote_wf",
                columns: table => new
                {
                    id_lote = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_paso = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    responsable = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lote_wf", x => x.id_lote);
                });

            migrationBuilder.CreateTable(
                name: "tareas",
                columns: table => new
                {
                    id_tarea = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_workflow = table.Column<long>(type: "bigint", nullable: false),
                    asunto = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    prioridad = table.Column<string>(type: "text", nullable: false),
                    paso = table.Column<int>(type: "integer", nullable: false),
                    responsable = table.Column<string>(type: "text", nullable: false),
                    remitente = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<short>(type: "smallint", nullable: false),
                    aviso = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_aviso = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_recepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_ejecucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    dias = table.Column<int>(type: "integer", nullable: false),
                    horas = table.Column<int>(type: "integer", nullable: false),
                    minutos = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    paso_destino = table.Column<int>(type: "integer", nullable: false),
                    paso_siguiente = table.Column<int>(type: "integer", nullable: false),
                    responsable_paso_siguiente = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tareas", x => x.id_tarea);
                });

            migrationBuilder.CreateTable(
                name: "tareas_lote",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_lote = table.Column<long>(type: "bigint", nullable: false),
                    id_tarea = table.Column<long>(type: "bigint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tareas_lote", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agenda");

            migrationBuilder.DropTable(
                name: "carpeta_tarea");

            migrationBuilder.DropTable(
                name: "comentarios_tarea");

            migrationBuilder.DropTable(
                name: "contenido_carpeta");

            migrationBuilder.DropTable(
                name: "lote_wf");

            migrationBuilder.DropTable(
                name: "tareas");

            migrationBuilder.DropTable(
                name: "tareas_lote");
        }
    }
}
