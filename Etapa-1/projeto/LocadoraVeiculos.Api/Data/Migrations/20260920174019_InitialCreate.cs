using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocadoraVeiculos.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasVeiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasVeiculo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "char(11)", unicode: false, fixedLength: true, maxLength: 11, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Telefone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    DataNascimento = table.Column<DateOnly>(type: "date", nullable: false),
                    NumeroCnh = table.Column<string>(type: "varchar(11)", unicode: false, maxLength: 11, nullable: false),
                    ValidadeCnh = table.Column<DateOnly>(type: "date", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fabricantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaisOrigem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fabricantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FabricanteId = table.Column<int>(type: "int", nullable: false),
                    CategoriaVeiculoId = table.Column<int>(type: "int", nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Placa = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: false),
                    Chassi = table.Column<string>(type: "varchar(17)", unicode: false, maxLength: 17, nullable: false),
                    AnoFabricacao = table.Column<int>(type: "int", nullable: false),
                    Quilometragem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.CheckConstraint("CK_Veiculos_AnoFabricacao", "[AnoFabricacao] BETWEEN 1886 AND 2100");
                    table.CheckConstraint("CK_Veiculos_Quilometragem", "[Quilometragem] >= 0");
                    table.ForeignKey(
                        name: "FK_Veiculos_CategoriasVeiculo_CategoriaVeiculoId",
                        column: x => x.CategoriaVeiculoId,
                        principalTable: "CategoriasVeiculo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Veiculos_Fabricantes_FabricanteId",
                        column: x => x.FabricanteId,
                        principalTable: "Fabricantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alugueis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VeiculoId = table.Column<int>(type: "int", nullable: false),
                    DataRetirada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataPrevistaDevolucao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataDevolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    QuilometragemInicial = table.Column<int>(type: "int", nullable: false),
                    QuilometragemFinal = table.Column<int>(type: "int", nullable: true),
                    ValorDiaria = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alugueis", x => x.Id);
                    table.CheckConstraint("CK_Alugueis_DataDevolucao", "[DataDevolucao] IS NULL OR [DataDevolucao] >= [DataRetirada]");
                    table.CheckConstraint("CK_Alugueis_DevolucaoCompleta", "([DataDevolucao] IS NULL AND [QuilometragemFinal] IS NULL AND [ValorTotal] IS NULL) OR ([DataDevolucao] IS NOT NULL AND [QuilometragemFinal] IS NOT NULL AND [ValorTotal] IS NOT NULL)");
                    table.CheckConstraint("CK_Alugueis_PeriodoPrevisto", "[DataPrevistaDevolucao] > [DataRetirada]");
                    table.CheckConstraint("CK_Alugueis_QuilometragemFinal", "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");
                    table.CheckConstraint("CK_Alugueis_QuilometragemInicial", "[QuilometragemInicial] >= 0");
                    table.CheckConstraint("CK_Alugueis_ValorDiaria", "[ValorDiaria] > 0");
                    table.CheckConstraint("CK_Alugueis_ValorTotal", "[ValorTotal] IS NULL OR [ValorTotal] >= 0");
                    table.ForeignKey(
                        name: "FK_Alugueis_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alugueis_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alugueis_ClienteId_DataRetirada",
                table: "Alugueis",
                columns: new[] { "ClienteId", "DataRetirada" });

            migrationBuilder.CreateIndex(
                name: "IX_Alugueis_VeiculoId_DataRetirada_DataPrevistaDevolucao",
                table: "Alugueis",
                columns: new[] { "VeiculoId", "DataRetirada", "DataPrevistaDevolucao" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasVeiculo_Nome",
                table: "CategoriasVeiculo",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Email",
                table: "Clientes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NumeroCnh",
                table: "Clientes",
                column: "NumeroCnh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fabricantes_Nome",
                table: "Fabricantes",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_CategoriaVeiculoId",
                table: "Veiculos",
                column: "CategoriaVeiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Chassi",
                table: "Veiculos",
                column: "Chassi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_FabricanteId",
                table: "Veiculos",
                column: "FabricanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_Placa",
                table: "Veiculos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alugueis");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.DropTable(
                name: "CategoriasVeiculo");

            migrationBuilder.DropTable(
                name: "Fabricantes");
        }
    }
}
