using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Applications_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataBases",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBases", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DataBases_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Operations_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "ID", "Name" },
                values: new object[,]
                {
                    { new Guid("27af42de-1923-409d-91e4-3228a02f8f46"), "Counterparty" },
                    { new Guid("a7dd8aa5-3c72-4f6b-a31b-4abbdfa096b4"), "Another" }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "ID", "Name", "ProjectID" },
                values: new object[,]
                {
                    { new Guid("304b915e-d9ce-42a2-afa9-222f42a94f05"), "CounterpartyJob", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("c64fa529-34c0-431b-8e65-184c69c0f30c"), "CounterpartyApi", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("f2d618ab-3f0b-408d-9455-ceb97a4076a1"), "CounterpartyWeb", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") }
                });

            migrationBuilder.InsertData(
                table: "DataBases",
                columns: new[] { "ID", "Name", "ProjectID" },
                values: new object[,]
                {
                    { new Guid("1040744a-ad62-4db3-a449-ede765cecb7d"), "1-Counterparty", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("400b30d8-3db4-4e5c-b989-b54489deec98"), "4-Common", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("83d05b8d-69f6-485f-aa0e-ef447403b6aa"), "2-AccountManagement", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("d3de1b89-00c1-410e-aa25-7d17b5e5c40e"), "3-CCS_Oracle_views", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") }
                });

            migrationBuilder.InsertData(
                table: "Operations",
                columns: new[] { "ID", "Name", "ProjectID" },
                values: new object[,]
                {
                    { new Guid("3bbfff92-d6a3-4d7b-8c9f-5258bd88167b"), "Applications", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("8850acfe-e5d3-49b2-87b4-1d439ebcdb24"), "Reports", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") },
                    { new Guid("b7ebb3de-cae7-4928-b372-c62cf42091e8"), "DataBases", new Guid("27af42de-1923-409d-91e4-3228a02f8f46") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProjectID",
                table: "Applications",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_DataBases_ProjectID",
                table: "DataBases",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_ProjectID",
                table: "Operations",
                column: "ProjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "DataBases");

            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
