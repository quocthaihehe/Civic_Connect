using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase7_CommunityAndPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ForumPosts");

            migrationBuilder.AddColumn<int>(
                name: "CommentCount",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnteredTrendingAt",
                table: "ForumPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "PopularityScore",
                table: "ForumPosts",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ForumPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ForumPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "ForumComments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Depth",
                table: "ForumComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IssueId",
                table: "ForumComments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "ForumComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "ForumComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PointsDelta = table.Column<int>(type: "int", nullable: false),
                    TrustScoreDelta = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrendingTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrendingTopics", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9081));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9084));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9086));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9011));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9013));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 9, 15, 8, 4, DateTimeKind.Utc).AddTicks(9013));

            migrationBuilder.CreateIndex(
                name: "IX_ForumComments_IssueId",
                table: "ForumComments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComments_ParentCommentId",
                table: "ForumComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId",
                table: "PointTransactions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumComments_ForumComments_ParentCommentId",
                table: "ForumComments",
                column: "ParentCommentId",
                principalTable: "ForumComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ForumComments_Issues_IssueId",
                table: "ForumComments",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumComments_ForumComments_ParentCommentId",
                table: "ForumComments");

            migrationBuilder.DropForeignKey(
                name: "FK_ForumComments_Issues_IssueId",
                table: "ForumComments");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "TrendingTopics");

            migrationBuilder.DropIndex(
                name: "IX_ForumComments_IssueId",
                table: "ForumComments");

            migrationBuilder.DropIndex(
                name: "IX_ForumComments_ParentCommentId",
                table: "ForumComments");

            migrationBuilder.DropColumn(
                name: "CommentCount",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "EnteredTrendingAt",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "PopularityScore",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "Depth",
                table: "ForumComments");

            migrationBuilder.DropColumn(
                name: "IssueId",
                table: "ForumComments");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "ForumComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "ForumComments");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ForumPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "ForumComments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(2039));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(2044));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(2046));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(1753));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(1762));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 23, 6, 49, 18, 897, DateTimeKind.Utc).AddTicks(1763));
        }
    }
}
