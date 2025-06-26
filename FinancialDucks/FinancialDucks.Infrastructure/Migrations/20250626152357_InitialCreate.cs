using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialDucks.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    HierarchyId = table.Column<HierarchyId>(type: "hierarchyid", nullable: false),
                    Starred = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionSourceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSourceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BudgetLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Budget = table.Column<decimal>(type: "money", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetLines_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SubstringMatch = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    AmountMin = table.Column<decimal>(type: "money", nullable: true),
                    AmountMax = table.Column<decimal>(type: "money", nullable: true),
                    DateMin = table.Column<DateTime>(type: "datetime", nullable: true),
                    DateMax = table.Column<DateTime>(type: "datetime", nullable: true),
                    Priority = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryRules_Categories",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransactionSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionSources_TransactionSourceTypes",
                        column: x => x.TypeId,
                        principalTable: "TransactionSourceTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScraperCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Selector = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    Text = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TimeoutSeconds = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((30))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScraperCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScraperCommands_TransactionSources",
                        column: x => x.SourceId,
                        principalTable: "TransactionSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SourceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceSnapshots_TransactionSources",
                        column: x => x.SourceId,
                        principalTable: "TransactionSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", unicode: false, maxLength: 250, nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionSources",
                        column: x => x.SourceId,
                        principalTable: "TransactionSources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetLines_CategoryId",
                table: "BudgetLines",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20220402-121716",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryRules_CategoryId",
                table: "CategoryRules",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ScraperCommands",
                table: "ScraperCommands",
                columns: new[] { "SourceId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SourceSnapshots_SourceId",
                table: "SourceSnapshots",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceId",
                table: "Transactions",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionSources_TypeId",
                table: "TransactionSources",
                column: "TypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetLines");

            migrationBuilder.DropTable(
                name: "CategoryRules");

            migrationBuilder.DropTable(
                name: "ScraperCommands");

            migrationBuilder.DropTable(
                name: "SourceSnapshots");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "TransactionSources");

            migrationBuilder.DropTable(
                name: "TransactionSourceTypes");
        }
    }
}
