using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8",
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEGUrhuLXVMtD5o6jaa8JDUduAgPQFlY6biIUDgo7QQNYCSkoBju+mphU+qp8CnsB9Q==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "eac0adc0-c5fc-4765-9f9d-b1ff1f8794c8",
                column: "PasswordHash",
                value: null);
        }
    }
}
