using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.Workflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_paso",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_paso = table.Column<int>(type: "integer", nullable: false),
                    id_opcion = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_paso", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "evidencias",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_opcion = table.Column<int>(type: "integer", nullable: false),
                    id_workflow = table.Column<long>(type: "bigint", nullable: false),
                    id_tarea = table.Column<long>(type: "bigint", nullable: false),
                    parametros = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    id_transaccion = table.Column<string>(type: "text", nullable: true),
                    respuesta_json = table.Column<string>(type: "text", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    documento_html = table.Column<string>(type: "text", nullable: true),
                    nombre_evidencia = table.Column<string>(type: "text", nullable: true),
                    procesado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evidencias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "formulario_workflow",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_workflow = table.Column<long>(type: "bigint", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_registro = table.Column<long>(type: "bigint", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formulario_workflow", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grupos_wf",
                columns: table => new
                {
                    id_grupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos_wf", x => x.id_grupo);
                });

            migrationBuilder.CreateTable(
                name: "miembros_grupo_wf",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_grupo = table.Column<int>(type: "integer", nullable: false),
                    miembro = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_miembros_grupo_wf", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "paso",
                columns: table => new
                {
                    id_paso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_proceso = table.Column<int>(type: "integer", nullable: false),
                    paso = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    prioridad = table.Column<string>(type: "text", nullable: false),
                    plazo = table.Column<int>(type: "integer", nullable: false),
                    unidad_plazo = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paso", x => x.id_paso);
                });

            migrationBuilder.CreateTable(
                name: "posibles_pasos_devolucion",
                columns: table => new
                {
                    paso = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    id_proceso = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posibles_pasos_devolucion", x => new { x.paso, x.descripcion, x.id_proceso });
                });

            migrationBuilder.CreateTable(
                name: "proceso",
                columns: table => new
                {
                    id_proceso = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    proceso = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proceso", x => x.id_proceso);
                });

            migrationBuilder.CreateTable(
                name: "workflow",
                columns: table => new
                {
                    id_workflow = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_proceso = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_registro = table.Column<long>(type: "bigint", nullable: false),
                    fecha_finalizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    id_workflow_v10 = table.Column<long>(type: "bigint", nullable: true),
                    workflow_v10 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow", x => x.id_workflow);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_paso");

            migrationBuilder.DropTable(
                name: "evidencias");

            migrationBuilder.DropTable(
                name: "formulario_workflow");

            migrationBuilder.DropTable(
                name: "grupos_wf");

            migrationBuilder.DropTable(
                name: "miembros_grupo_wf");

            migrationBuilder.DropTable(
                name: "paso");

            migrationBuilder.DropTable(
                name: "posibles_pasos_devolucion");

            migrationBuilder.DropTable(
                name: "proceso");

            migrationBuilder.DropTable(
                name: "workflow");
        }
    }
}
