using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VaultShop.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyFiscalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CuitSnapshot",
                table: "OrderHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomicilioFiscalSnapshot",
                table: "OrderHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazonSocialSnapshot",
                table: "OrderHeaders",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StreetAddress",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cuit",
                table: "Companies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DomicilioFiscal",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazonSocial",
                table: "Companies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CuitSnapshot",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "DomicilioFiscalSnapshot",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "RazonSocialSnapshot",
                table: "OrderHeaders");

            migrationBuilder.DropColumn(
                name: "Cuit",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "DomicilioFiscal",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "RazonSocial",
                table: "Companies");

            migrationBuilder.AlterColumn<string>(
                name: "StreetAddress",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Companies",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
