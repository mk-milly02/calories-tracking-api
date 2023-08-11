using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_api.infrastructure.migrations
{
    /// <inheritdoc />
    public partial class ChangedDateTimeToCreatedInMealEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ab93c71a-8d2e-40bb-9539-e4633e387f41"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c729cf1b-380d-486b-9fbe-00628aaa6fe6"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("cb5e0537-7d8d-4d06-90e2-013338d4645c"));

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Meals",
                newName: "Created");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("1be4d2b1-6757-42f5-bbf2-a75149278703"), null, "UserManager", "USERMANAGER" },
                    { new Guid("585c059a-6891-458c-9dae-e54dd84a7b0a"), null, "RegularUser", "REGULARUSER" },
                    { new Guid("9ca8b249-6a2e-47f4-977c-794886bb6614"), null, "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1be4d2b1-6757-42f5-bbf2-a75149278703"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("585c059a-6891-458c-9dae-e54dd84a7b0a"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9ca8b249-6a2e-47f4-977c-794886bb6614"));

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Meals",
                newName: "DateTime");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("ab93c71a-8d2e-40bb-9539-e4633e387f41"), null, "UserManager", "USERMANAGER" },
                    { new Guid("c729cf1b-380d-486b-9fbe-00628aaa6fe6"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("cb5e0537-7d8d-4d06-90e2-013338d4645c"), null, "RegularUser", "REGULARUSER" }
                });
        }
    }
}
