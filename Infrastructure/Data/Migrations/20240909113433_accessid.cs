using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class accessid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ID",
                table: "Accesses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accesses",
                table: "Accesses",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_ParentId",
                table: "Accesses",
                column: "ParentId",
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Accesses_Accesses_ParentId",
                table: "Accesses",
                column: "ParentId",
                principalTable: "Accesses",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accesses_Accesses_ParentId",
                table: "Accesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Accesses",
                table: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_ParentId",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Accesses");
        }
    }
}
