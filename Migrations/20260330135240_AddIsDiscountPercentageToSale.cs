using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Galaxium.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDiscountPercentageToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDiscountPercentage",
                table: "Sale",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PaymentMethod",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "IsDiscountPercentage",
                table: "Sale");
        }
    }
}
