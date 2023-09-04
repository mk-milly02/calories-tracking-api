using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_tracking.infrastructure.persistence.migrations
{
    /// <inheritdoc />
    public partial class ChangedonetomanyreletionbetweenUserandMeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("51ea5cd5-1e6c-43b6-ac24-b7da7c2ba052"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("74c5dfd0-59c8-4840-87ec-3b97aba200b8"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("d8b176f4-0417-45d0-9d03-d4a4e3f1e4c7"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("34d867ac-36f7-4d8f-b952-bc421edb3050"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ad765b68-eea3-4a5d-a751-37df66f8daa9"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("e299c2cb-e31a-4935-8ca7-3666d8052643"), null, "RegularUser", "REGULARUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("34d867ac-36f7-4d8f-b952-bc421edb3050"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ad765b68-eea3-4a5d-a751-37df66f8daa9"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e299c2cb-e31a-4935-8ca7-3666d8052643"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("51ea5cd5-1e6c-43b6-ac24-b7da7c2ba052"), null, "RegularUser", "REGULARUSER" },
                    { new Guid("74c5dfd0-59c8-4840-87ec-3b97aba200b8"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("d8b176f4-0417-45d0-9d03-d4a4e3f1e4c7"), null, "UserManager", "USERMANAGER" }
                });
        }
    }
}
