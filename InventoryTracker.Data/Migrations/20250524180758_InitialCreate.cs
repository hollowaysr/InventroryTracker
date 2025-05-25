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
            // Tables already exist in TestApps database - no need to create them
            // This migration is for code-first compatibility only
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Tables already exist in TestApps database - no need to drop them
            // This migration is for code-first compatibility only
        }
    }
}
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
