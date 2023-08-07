using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_api.persistence.migrations
{
    /// <inheritdoc />
    public partial class SeedAdministrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("96065e29-03a2-4641-9dbb-68897b4772b9"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("979ea76d-ba2b-43fa-8849-ee160c3f30cd"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c47c2559-ad11-4e6f-9f33-fbb89ae5d367"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("103fe125-b44a-45d4-821d-5a573b634bd9"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("66fd83bc-4f01-4ef5-b0ad-104057fa3e1a"), null, "RegularUser", "REGULARUSER" },
                    { new Guid("f8f5eab9-78d7-48bc-9a73-31ca772d9beb"), null, "UserManager", "USERMANAGER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "ExpectedNumberOfCaloriesPerDay", "FirstName", "IsCaloriesDeficient", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PasswordSalt", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("2d6b5682-87fb-4397-bb62-9b92aab97a8f"), 0, "4f9cb62f-241f-46da-8cba-00d0de7e7ffa", "admin@calories-tracker.com", false, 0.0, "Jonas", false, "Ababio", false, null, null, null, "AQAAAAIAAYagAAAAEDq2lCAr+Y1NRaerv3ZlnKXaop7c4MDdJ3OojDVD7zJP84NChAwowikVnQrQcYflcQ==", "nSSNzksphtU/5PuGApmYfEuX6+REaX3Cz0FoQZ0zZrg=", null, false, null, false, "admin@calories-tracker.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("103fe125-b44a-45d4-821d-5a573b634bd9"), new Guid("2d6b5682-87fb-4397-bb62-9b92aab97a8f") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("66fd83bc-4f01-4ef5-b0ad-104057fa3e1a"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("f8f5eab9-78d7-48bc-9a73-31ca772d9beb"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("103fe125-b44a-45d4-821d-5a573b634bd9"), new Guid("2d6b5682-87fb-4397-bb62-9b92aab97a8f") });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("103fe125-b44a-45d4-821d-5a573b634bd9"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("2d6b5682-87fb-4397-bb62-9b92aab97a8f"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("96065e29-03a2-4641-9dbb-68897b4772b9"), null, "UserManager", "USERMANAGER" },
                    { new Guid("979ea76d-ba2b-43fa-8849-ee160c3f30cd"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("c47c2559-ad11-4e6f-9f33-fbb89ae5d367"), null, "RegularUser", "REGULARUSER" }
                });
        }
    }
}
