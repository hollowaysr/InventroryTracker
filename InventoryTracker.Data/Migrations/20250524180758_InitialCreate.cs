using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "List",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SystemRef = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    ledger_start_transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    ledger_end_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    ledger_start_sequence_number = table.Column<long>(type: "bigint", nullable: false),
                    ledger_end_sequence_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_List", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RFID",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RFID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Size = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ledger_start_transaction_id = table.Column<long>(type: "bigint", nullable: false),
                    ledger_end_transaction_id = table.Column<long>(type: "bigint", nullable: true),
                    ledger_start_sequence_number = table.Column<long>(type: "bigint", nullable: false),
                    ledger_end_sequence_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RFID", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RFID_List_ListId",
                        column: x => x.ListId,
                        principalTable: "List",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_List_Name",
                table: "List",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_List_SystemRef",
                table: "List",
                column: "SystemRef");

            migrationBuilder.CreateIndex(
                name: "IX_RFID_ListId",
                table: "RFID",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_RFID_Name",
                table: "RFID",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RFID_Rfid",
                table: "RFID",
                column: "RFID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RFID");

            migrationBuilder.DropTable(
                name: "List");
        }
    }
}
