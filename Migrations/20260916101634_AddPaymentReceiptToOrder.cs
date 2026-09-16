using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace El_Shaib.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceiptToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReceiptUrl",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionReference",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentReceiptUrl",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TransactionReference",
                table: "Orders");
        }
    }
}
