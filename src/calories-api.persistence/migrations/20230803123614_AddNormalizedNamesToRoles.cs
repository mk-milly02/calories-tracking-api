using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_api.persistence.migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedNamesToRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("3f374717-f05d-4d88-a5e8-e7010eec8e59"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9422d0bc-3829-4cf7-92c9-6e4a5a38ccb2"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b679fa86-342f-4aa8-83ea-81ab9b514530"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("c2729ce3-be68-4a50-aba5-a83b949da03e"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("e94c5353-118e-4ded-b8f0-cb415765b93f"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ed40a435-5032-4deb-ad62-1e2f4a2cbdef"), null, "RegularUser", "REGULARUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c2729ce3-be68-4a50-aba5-a83b949da03e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("e94c5353-118e-4ded-b8f0-cb415765b93f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ed40a435-5032-4deb-ad62-1e2f4a2cbdef"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("3f374717-f05d-4d88-a5e8-e7010eec8e59"), null, "UserManager", null },
                    { new Guid("9422d0bc-3829-4cf7-92c9-6e4a5a38ccb2"), null, "Administrator", null },
                    { new Guid("b679fa86-342f-4aa8-83ea-81ab9b514530"), null, "RegularUser", null }
                });
        }
    }
}
