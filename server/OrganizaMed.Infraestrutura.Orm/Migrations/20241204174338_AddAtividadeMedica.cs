using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizaMed.Infraestrutura.Orm.Migrations
{
    /// <inheritdoc />
    public partial class AddAtividadeMedica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBAtividadeMedica",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Termino = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoAtividade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBAtividadeMedica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TBAtividadeMedica_TBMedico",
                columns: table => new
                {
                    AtividadesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicosId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBAtividadeMedica_TBMedico", x => new { x.AtividadesId, x.MedicosId });
                    table.ForeignKey(
                        name: "FK_TBAtividadeMedica_TBMedico_TBAtividadeMedica_AtividadesId",
                        column: x => x.AtividadesId,
                        principalTable: "TBAtividadeMedica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TBAtividadeMedica_TBMedico_TBMedico_MedicosId",
                        column: x => x.MedicosId,
                        principalTable: "TBMedico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBAtividadeMedica_TBMedico_MedicosId",
                table: "TBAtividadeMedica_TBMedico",
                column: "MedicosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBAtividadeMedica_TBMedico");

            migrationBuilder.DropTable(
                name: "TBAtividadeMedica");
        }
    }
}
