using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace calories_tracking.infrastructure.persistence.migrations
{
    /// <inheritdoc />
    public partial class DefinedOneToManyRelationshipOnMealEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AspNetUsers_Id",
                table: "Meals");

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
                    { new Guid("7f760474-7dc6-4416-b867-55c2acaf0997"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ccd56605-d41b-4064-a79a-df5f0627708f"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("dcaa32bc-cf45-4059-bcd9-bbc428956d41"), null, "RegularUser", "REGULARUSER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meals_UserId",
                table: "Meals",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AspNetUsers_UserId",
                table: "Meals",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AspNetUsers_UserId",
                table: "Meals");

            migrationBuilder.DropIndex(
                name: "IX_Meals_UserId",
                table: "Meals");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("7f760474-7dc6-4416-b867-55c2acaf0997"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ccd56605-d41b-4064-a79a-df5f0627708f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("dcaa32bc-cf45-4059-bcd9-bbc428956d41"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("34d867ac-36f7-4d8f-b952-bc421edb3050"), null, "UserManager", "USERMANAGER" },
                    { new Guid("ad765b68-eea3-4a5d-a751-37df66f8daa9"), null, "Administrator", "ADMINISTRATOR" },
                    { new Guid("e299c2cb-e31a-4935-8ca7-3666d8052643"), null, "RegularUser", "REGULARUSER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AspNetUsers_Id",
                table: "Meals",
                column: "Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
