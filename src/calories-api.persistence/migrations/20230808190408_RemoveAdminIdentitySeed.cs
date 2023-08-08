using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_api.persistence.migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminIdentitySeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("76d8f116-27df-4d84-be5d-c35405fadfbc"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ccbf03b7-02b4-478f-a60a-8e688807aad0"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("42a54c78-9670-4162-9f16-e8a65bd207e1"), new Guid("df265af8-8b76-400d-a6e1-0eb0957ff485") });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("42a54c78-9670-4162-9f16-e8a65bd207e1"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("df265af8-8b76-400d-a6e1-0eb0957ff485"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("05d8587b-96ae-4489-9bd7-8c38a5e8b357"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("1d402d40-c923-4632-a835-3bec106a294c"), null, "UserManager", "USERMANAGER" },
                    { new Guid("86ddb818-cb2c-4a6f-b777-6795487e7266"), null, "RegularUser", "REGULARUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("05d8587b-96ae-4489-9bd7-8c38a5e8b357"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1d402d40-c923-4632-a835-3bec106a294c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("86ddb818-cb2c-4a6f-b777-6795487e7266"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("42a54c78-9670-4162-9f16-e8a65bd207e1"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("76d8f116-27df-4d84-be5d-c35405fadfbc"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ccbf03b7-02b4-478f-a60a-8e688807aad0"), null, "RegularUser", "REGULARUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "ExpectedNumberOfCaloriesPerDay", "FirstName", "IsCaloriesDeficient", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PasswordSalt", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("df265af8-8b76-400d-a6e1-0eb0957ff485"), 0, "99cc62ef-2a19-4fee-925e-c41c2a31494c", "admin@calories-tracker.com", false, 0.0, "Jonas", false, "Ababio", false, null, "ADMIN@CALORIES-TRACKER.COM", "ADMIN", "AQAAAAIAAYagAAAAEFlD60z5FnXiAYxL7jCCp9/klOXTsxciYIhCXq3RfTnUMQpByXfCWVRwQ86eNf7DjA==", "gPCpf6CWIyGuaEnuQSUr7SJeOYUjzaR0u4LH0y3dP7M=", null, false, null, false, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("42a54c78-9670-4162-9f16-e8a65bd207e1"), new Guid("df265af8-8b76-400d-a6e1-0eb0957ff485") });
        }
    }
}
