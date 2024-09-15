using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class accessid2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Accesses_Accesses_ParentId",
            //    table: "Accesses");

            //migrationBuilder.DropIndex(
            //    name: "IX_Accesses_ParentId",
            //    table: "Accesses");

            //migrationBuilder.AddColumn<long>(
            //    name: "ParentAccessID",
            //    table: "Accesses",
            //    type: "bigint",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Accesses_ParentAccessID",
            //    table: "Accesses",
            //    column: "ParentAccessID");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Accesses_Accesses_ParentAccessID",
            //    table: "Accesses",
            //    column: "ParentAccessID",
            //    principalTable: "Accesses",
            //    principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accesses_Accesses_ParentAccessID",
                table: "Accesses");

            migrationBuilder.DropIndex(
                name: "IX_Accesses_ParentAccessID",
                table: "Accesses");

            migrationBuilder.DropColumn(
                name: "ParentAccessID",
                table: "Accesses");

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
    }
}
