using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scheduly.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsaasIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AsaasPaymentId",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceUrl",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PixQrCodeUrl",
                table: "transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddressNumber",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasApiKey",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasWalletId",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complement",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfCnpj",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighborhood",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsaasCustomerId",
                table: "customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CpfCnpj",
                table: "customers",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsaasPaymentId",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "InvoiceUrl",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "PixQrCodeUrl",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "AddressNumber",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "AsaasApiKey",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "AsaasWalletId",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "City",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Complement",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "CpfCnpj",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Neighborhood",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "State",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "AsaasCustomerId",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CpfCnpj",
                table: "customers");
        }
    }
}
