using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HidroRec.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ScaleReadinessIndexesAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_IndicadoresClimaticos_FonteDadoId",
                table: "IndicadoresClimaticos");

            migrationBuilder.DropIndex(
                name: "IX_HistoricosReporte_ReporteId",
                table: "HistoricosReporte");

            migrationBuilder.DropIndex(
                name: "IX_Bairros_RegiaoId",
                table: "Bairros");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reportes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Severidade",
                table: "Reportes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Reportes",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Regioes",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "LogsSistema",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Evento",
                table: "LogsSistema",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Contexto",
                table: "LogsSistema",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "IndicadoresClimaticos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Bairros",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntidadeId",
                table: "Auditorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Entidade",
                table: "Auditorias",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Criticidade",
                table: "Alertas",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Alertas",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId_Ativo_DataCriacao",
                table: "Usuarios",
                columns: new[] { "PerfilId", "Ativo", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_Bairro_DataOcorrencia",
                table: "Reportes",
                columns: new[] { "Excluido", "BairroId", "DataOcorrencia" });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_DashboardLookup",
                table: "Reportes",
                columns: new[] { "Excluido", "DataOcorrencia", "Status", "Severidade" });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_Regiao_DataOcorrencia",
                table: "Reportes",
                columns: new[] { "Excluido", "RegiaoId", "DataOcorrencia" });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_Usuario_DataCriacao",
                table: "Reportes",
                columns: new[] { "Excluido", "UsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Regioes_Nome",
                table: "Regioes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogsSistema_Contexto_DataCriacao",
                table: "LogsSistema",
                columns: new[] { "Contexto", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_LogsSistema_Nivel_Evento_DataCriacao",
                table: "LogsSistema",
                columns: new[] { "Nivel", "Evento", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_IndicadoresClimaticos_Fonte_Categoria_Referencia",
                table: "IndicadoresClimaticos",
                columns: new[] { "FonteDadoId", "Categoria", "ReferenciaEm" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosReporte_Reporte_DataAlteracao",
                table: "HistoricosReporte",
                columns: new[] { "ReporteId", "DataAlteracao" });

            migrationBuilder.CreateIndex(
                name: "IX_Bairros_RegiaoId_Nome",
                table: "Bairros",
                columns: new[] { "RegiaoId", "Nome" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Entidade_EntidadeId_DataCriacao",
                table: "Auditorias",
                columns: new[] { "Entidade", "EntidadeId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Usuario_DataCriacao",
                table: "Auditorias",
                columns: new[] { "UsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_Ativo_Criticidade_Bairro_DataCriacao",
                table: "Alertas",
                columns: new[] { "Ativo", "Criticidade", "BairroId", "DataCriacao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_PerfilId_Ativo_DataCriacao",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_Bairro_DataOcorrencia",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_DashboardLookup",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_Regiao_DataOcorrencia",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_Usuario_DataCriacao",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Regioes_Nome",
                table: "Regioes");

            migrationBuilder.DropIndex(
                name: "IX_LogsSistema_Contexto_DataCriacao",
                table: "LogsSistema");

            migrationBuilder.DropIndex(
                name: "IX_LogsSistema_Nivel_Evento_DataCriacao",
                table: "LogsSistema");

            migrationBuilder.DropIndex(
                name: "IX_IndicadoresClimaticos_Fonte_Categoria_Referencia",
                table: "IndicadoresClimaticos");

            migrationBuilder.DropIndex(
                name: "IX_HistoricosReporte_Reporte_DataAlteracao",
                table: "HistoricosReporte");

            migrationBuilder.DropIndex(
                name: "IX_Bairros_RegiaoId_Nome",
                table: "Bairros");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Entidade_EntidadeId_DataCriacao",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Auditorias_Usuario_DataCriacao",
                table: "Auditorias");

            migrationBuilder.DropIndex(
                name: "IX_Alertas_Ativo_Criticidade_Bairro_DataCriacao",
                table: "Alertas");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Alertas");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Severidade",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Regioes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "LogsSistema",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Evento",
                table: "LogsSistema",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Contexto",
                table: "LogsSistema",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "IndicadoresClimaticos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Bairros",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "EntidadeId",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Entidade",
                table: "Auditorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Criticidade",
                table: "Alertas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_PerfilId",
                table: "Usuarios",
                column: "PerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_IndicadoresClimaticos_FonteDadoId",
                table: "IndicadoresClimaticos",
                column: "FonteDadoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricosReporte_ReporteId",
                table: "HistoricosReporte",
                column: "ReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Bairros_RegiaoId",
                table: "Bairros",
                column: "RegiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");
        }
    }
}
