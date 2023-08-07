using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_api.persistence.migrations
{
    /// <inheritdoc />
    public partial class AutoGeneratePrimaryKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    { new Guid("96065e29-03a2-4641-9dbb-68897b4772b9"), null, "UserManager", "USERMANAGER" },
                    { new Guid("979ea76d-ba2b-43fa-8849-ee160c3f30cd"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("c47c2559-ad11-4e6f-9f33-fbb89ae5d367"), null, "RegularUser", "REGULARUSER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                    { new Guid("c2729ce3-be68-4a50-aba5-a83b949da03e"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("e94c5353-118e-4ded-b8f0-cb415765b93f"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ed40a435-5032-4deb-ad62-1e2f4a2cbdef"), null, "RegularUser", "REGULARUSER" }
                });
        }
    }
}
