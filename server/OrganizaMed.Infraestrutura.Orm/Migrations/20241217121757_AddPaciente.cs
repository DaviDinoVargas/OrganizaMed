using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizaMed.Infraestrutura.Orm.Migrations
{
    /// <inheritdoc />
    public partial class AddPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PacienteId",
                table: "TBAtividadeMedica",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TBPaciente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Cpf = table.Column<string>(type: "char(14)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Telefone = table.Column<string>(type: "varchar(15)", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBPaciente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TBPaciente_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBAtividadeMedica_PacienteId",
                table: "TBAtividadeMedica",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TBPaciente_UsuarioId",
                table: "TBPaciente",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_TBAtividadeMedica_TBPaciente_PacienteId",
                table: "TBAtividadeMedica",
                column: "PacienteId",
                principalTable: "TBPaciente",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TBAtividadeMedica_TBPaciente_PacienteId",
                table: "TBAtividadeMedica");

            migrationBuilder.DropTable(
                name: "TBPaciente");

            migrationBuilder.DropIndex(
                name: "IX_TBAtividadeMedica_PacienteId",
                table: "TBAtividadeMedica");

            migrationBuilder.DropColumn(
                name: "PacienteId",
                table: "TBAtividadeMedica");
        }
    }
}
