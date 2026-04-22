using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HidroRec.Backend.Migrations
{
    /// <inheritdoc />
    public partial class OperationalIntelligenceExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaMonitoradaId",
                table: "Reportes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Organizacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Segmento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Excluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AreasMonitoradas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrganizacaoId = table.Column<int>(type: "int", nullable: false),
                    BairroId = table.Column<int>(type: "int", nullable: true),
                    RegiaoId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    Vulnerabilidade = table.Column<int>(type: "int", nullable: false),
                    CriticidadeOperacional = table.Column<int>(type: "int", nullable: false),
                    Cobertura = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Excluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreasMonitoradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AreasMonitoradas_Bairros_BairroId",
                        column: x => x.BairroId,
                        principalTable: "Bairros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_AreasMonitoradas_Organizacoes_OrganizacaoId",
                        column: x => x.OrganizacaoId,
                        principalTable: "Organizacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AreasMonitoradas_Regioes_RegiaoId",
                        column: x => x.RegiaoId,
                        principalTable: "Regioes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AtivosMonitorados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrganizacaoId = table.Column<int>(type: "int", nullable: false),
                    AreaMonitoradaId = table.Column<int>(type: "int", nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,6)", precision: 10, scale: 6, nullable: false),
                    Sensibilidade = table.Column<int>(type: "int", nullable: false),
                    StatusOperacional = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Excluido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtivosMonitorados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AtivosMonitorados_AreasMonitoradas_AreaMonitoradaId",
                        column: x => x.AreaMonitoradaId,
                        principalTable: "AreasMonitoradas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AtivosMonitorados_Organizacoes_OrganizacaoId",
                        column: x => x.OrganizacaoId,
                        principalTable: "Organizacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_AreaMonitorada_DataOcorrencia",
                table: "Reportes",
                columns: new[] { "Excluido", "AreaMonitoradaId", "DataOcorrencia" });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_AreaMonitoradaId",
                table: "Reportes",
                column: "AreaMonitoradaId");

            migrationBuilder.CreateIndex(
                name: "IX_AreasMonitoradas_Ativa_Organizacao_Regiao",
                table: "AreasMonitoradas",
                columns: new[] { "Ativa", "OrganizacaoId", "RegiaoId" });

            migrationBuilder.CreateIndex(
                name: "IX_AreasMonitoradas_BairroId",
                table: "AreasMonitoradas",
                column: "BairroId");

            migrationBuilder.CreateIndex(
                name: "IX_AreasMonitoradas_Codigo",
                table: "AreasMonitoradas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AreasMonitoradas_OrganizacaoId",
                table: "AreasMonitoradas",
                column: "OrganizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AreasMonitoradas_RegiaoId",
                table: "AreasMonitoradas",
                column: "RegiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AtivosMonitorados_AreaMonitoradaId",
                table: "AtivosMonitorados",
                column: "AreaMonitoradaId");

            migrationBuilder.CreateIndex(
                name: "IX_AtivosMonitorados_Ativo_Organizacao_Area",
                table: "AtivosMonitorados",
                columns: new[] { "Ativo", "OrganizacaoId", "AreaMonitoradaId" });

            migrationBuilder.CreateIndex(
                name: "IX_AtivosMonitorados_Codigo",
                table: "AtivosMonitorados",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AtivosMonitorados_OrganizacaoId",
                table: "AtivosMonitorados",
                column: "OrganizacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizacoes_Codigo",
                table: "Organizacoes",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reportes_AreasMonitoradas_AreaMonitoradaId",
                table: "Reportes",
                column: "AreaMonitoradaId",
                principalTable: "AreasMonitoradas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reportes_AreasMonitoradas_AreaMonitoradaId",
                table: "Reportes");

            migrationBuilder.DropTable(
                name: "AtivosMonitorados");

            migrationBuilder.DropTable(
                name: "AreasMonitoradas");

            migrationBuilder.DropTable(
                name: "Organizacoes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_AreaMonitorada_DataOcorrencia",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_AreaMonitoradaId",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "AreaMonitoradaId",
                table: "Reportes");
        }
    }
}
