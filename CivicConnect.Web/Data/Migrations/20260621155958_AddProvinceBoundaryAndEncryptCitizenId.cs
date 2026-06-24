using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProvinceBoundaryAndEncryptCitizenId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvinceBoundaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ProvinceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MinLat = table.Column<double>(type: "float", nullable: false),
                    MaxLat = table.Column<double>(type: "float", nullable: false),
                    MinLng = table.Column<double>(type: "float", nullable: false),
                    MaxLng = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceBoundaries", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(5208));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(5216));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(5218));

            

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(4809));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(4818));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 21, 15, 59, 56, 642, DateTimeKind.Utc).AddTicks(4872));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvinceBoundaries");

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8627));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8632));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8633));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8548));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8549));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 17, 11, 38, 1, 936, DateTimeKind.Utc).AddTicks(8550));
        }
    }
}

