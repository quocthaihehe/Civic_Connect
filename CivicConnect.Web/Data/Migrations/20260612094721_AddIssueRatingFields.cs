using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIssueRatingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RatedAt",
                table: "Issues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Issues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RatingFeedback",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatedAt",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "RatingFeedback",
                table: "Issues");
        }
    }
}
