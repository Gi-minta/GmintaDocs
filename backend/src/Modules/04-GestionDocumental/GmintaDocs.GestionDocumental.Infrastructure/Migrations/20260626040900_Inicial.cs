using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmintaDocs.GestionDocumental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "archivos",
                columns: table => new
                {
                    id_archivo = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    archivo = table.Column<string>(type: "text", nullable: false),
                    extension = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha_documento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    directorio = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    id_primera_version = table.Column<long>(type: "bigint", nullable: false),
                    es_version_actual = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    id_archivo_principal = table.Column<long>(type: "bigint", nullable: false),
                    id_tipo_documento = table.Column<long>(type: "bigint", nullable: false),
                    bytes = table.Column<long>(type: "bigint", nullable: false),
                    etiquetas = table.Column<string>(type: "text", nullable: true),
                    unidad_conservacion = table.Column<string>(type: "text", nullable: true),
                    unidad_almacenamiento = table.Column<string>(type: "text", nullable: true),
                    id_formulario_v10 = table.Column<long>(type: "bigint", nullable: true),
                    id_registro_v10 = table.Column<long>(type: "bigint", nullable: true),
                    id_registro_archivo_v10 = table.Column<long>(type: "bigint", nullable: true),
                    id_directorio_v10 = table.Column<int>(type: "integer", nullable: true),
                    ruta_v10 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archivos", x => x.id_archivo);
                });

            migrationBuilder.CreateTable(
                name: "archivos_formulario",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_archivo = table.Column<long>(type: "bigint", nullable: false),
                    id_registro = table.Column<long>(type: "bigint", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_directorio = table.Column<long>(type: "bigint", nullable: true),
                    id_workflow = table.Column<long>(type: "bigint", nullable: true),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archivos_formulario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ayuda_almacenar",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_tipo_documento = table.Column<long>(type: "bigint", nullable: false),
                    id_directorio = table.Column<long>(type: "bigint", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ayuda_almacenar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "busqueda_archivos",
                columns: table => new
                {
                    id_archivo = table.Column<long>(type: "bigint", nullable: false),
                    archivo = table.Column<string>(type: "text", nullable: false),
                    extension = table.Column<string>(type: "text", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha_documento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    directorio = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    id_primera_version = table.Column<long>(type: "bigint", nullable: false),
                    es_version_actual = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<string>(type: "text", nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    id_archivo_principal = table.Column<long>(type: "bigint", nullable: false),
                    id_tipo_documento = table.Column<long>(type: "bigint", nullable: false),
                    bytes = table.Column<long>(type: "bigint", nullable: false),
                    etiquetas = table.Column<string>(type: "text", nullable: true),
                    id = table.Column<long>(type: "bigint", nullable: true),
                    id_registro = table.Column<long>(type: "bigint", nullable: true),
                    id_formulario = table.Column<long>(type: "bigint", nullable: true),
                    id_directorio = table.Column<long>(type: "bigint", nullable: true),
                    id_workflow = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tabla = table.Column<string>(type: "text", nullable: false),
                    id_registro = table.Column<long>(type: "bigint", nullable: false),
                    accion = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    comentario = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relacion_archivos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_archivo = table.Column<long>(type: "bigint", nullable: false),
                    id_relacion = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relacion_archivos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_documento",
                columns: table => new
                {
                    id_tipo_documento = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    controla_version = table.Column<bool>(type: "boolean", nullable: false),
                    dias_vigencia = table.Column<int>(type: "integer", nullable: false),
                    usuario = table.Column<string>(type: "text", nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    host = table.Column<string>(type: "text", nullable: false),
                    controla_vigencia = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_documento", x => x.id_tipo_documento);
                });

            migrationBuilder.CreateTable(
                name: "trd",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    id_tipo_documental = table.Column<long>(type: "bigint", nullable: true),
                    retencion_ag = table.Column<float>(type: "real", nullable: true),
                    retencion_ac = table.Column<float>(type: "real", nullable: true),
                    conservacion_total = table.Column<bool>(type: "boolean", nullable: false),
                    eliminacion = table.Column<bool>(type: "boolean", nullable: false),
                    microfilmacion = table.Column<bool>(type: "boolean", nullable: false),
                    seleccion = table.Column<bool>(type: "boolean", nullable: false),
                    porcentaje_seleccion = table.Column<float>(type: "real", nullable: true),
                    facilitativo = table.Column<bool>(type: "boolean", nullable: false),
                    facultativo = table.Column<bool>(type: "boolean", nullable: false),
                    sustantivo = table.Column<bool>(type: "boolean", nullable: false),
                    legal = table.Column<bool>(type: "boolean", nullable: false),
                    fiscal = table.Column<bool>(type: "boolean", nullable: false),
                    contable = table.Column<bool>(type: "boolean", nullable: false),
                    funcional = table.Column<bool>(type: "boolean", nullable: false),
                    administrativo = table.Column<bool>(type: "boolean", nullable: false),
                    historico = table.Column<bool>(type: "boolean", nullable: false),
                    cientifico = table.Column<bool>(type: "boolean", nullable: false),
                    cultural = table.Column<bool>(type: "boolean", nullable: false),
                    misional = table.Column<bool>(type: "boolean", nullable: false),
                    procedimiento = table.Column<string>(type: "text", nullable: true),
                    normatividad = table.Column<string>(type: "text", nullable: true),
                    observaciones = table.Column<string>(type: "text", nullable: true),
                    ley1581 = table.Column<bool>(type: "boolean", nullable: false),
                    retencion_electronica = table.Column<float>(type: "real", nullable: true),
                    eliminacion_electronica = table.Column<string>(type: "text", nullable: true),
                    fisico = table.Column<bool>(type: "boolean", nullable: false),
                    inmaterializado = table.Column<bool>(type: "boolean", nullable: false),
                    desmaterializado = table.Column<bool>(type: "boolean", nullable: false),
                    simple = table.Column<bool>(type: "boolean", nullable: false),
                    integro = table.Column<bool>(type: "boolean", nullable: false),
                    autentico = table.Column<bool>(type: "boolean", nullable: false),
                    firma_digital = table.Column<bool>(type: "boolean", nullable: false),
                    firma_biometrica = table.Column<bool>(type: "boolean", nullable: false),
                    estampado_cronologico = table.Column<bool>(type: "boolean", nullable: false),
                    seguridad = table.Column<string>(type: "text", nullable: true),
                    nivel_seguridad = table.Column<string>(type: "text", nullable: true),
                    pertenece_sgc = table.Column<bool>(type: "boolean", nullable: false),
                    id_formulario = table.Column<long>(type: "bigint", nullable: false),
                    id_registro = table.Column<long>(type: "bigint", nullable: false),
                    evidencia_niif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trd", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "archivos");

            migrationBuilder.DropTable(
                name: "archivos_formulario");

            migrationBuilder.DropTable(
                name: "ayuda_almacenar");

            migrationBuilder.DropTable(
                name: "busqueda_archivos");

            migrationBuilder.DropTable(
                name: "eventos");

            migrationBuilder.DropTable(
                name: "relacion_archivos");

            migrationBuilder.DropTable(
                name: "tipo_documento");

            migrationBuilder.DropTable(
                name: "trd");
        }
    }
}
