using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliciesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Excerpt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TagClass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssuingUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "Content", "Excerpt", "IsActive", "IssuingUnit", "PublishedDate", "Tag", "TagClass", "Title" },
                values: new object[,]
                {
                    { 1, "Nội dung chi tiết nghị định mới...", "Chính phủ vừa ban hành nghị định mới tăng mức phạt đối với các hành vi xả rác bừa bãi và lấn chiếm lòng lề đường tại đô thị.", true, "UBND Quận 1", new DateTime(2026, 6, 8, 3, 31, 5, 46, DateTimeKind.Utc).AddTicks(5409), "Luật mới", "tag-law", "Nghị định mới về xử phạt hành chính vi phạm môi trường đô thị" },
                    { 2, "Nội dung thông báo chi tiết...", "Thực hiện nếp sống văn minh đô thị, UBND Phường tổ chức đợt ra quân tổng vệ sinh các tuyến đường trọng điểm vào sáng Chủ Nhật tuần này.", true, "UBND Phường Bến Nghé", new DateTime(2026, 6, 9, 3, 31, 5, 46, DateTimeKind.Utc).AddTicks(5415), "Thông báo", "tag-notice", "Thông báo ra quân dọn dẹp vệ sinh môi trường trên địa bàn Phường Bến Nghé" },
                    { 3, "Nội dung chính sách chi tiết...", "Chương trình nâng cấp, chỉnh trang các tuyến vỉa hè trung tâm nhằm nâng cao mỹ quan đô thị và tạo không gian đi bộ an toàn cho người dân.", true, "UBND Quận 1", new DateTime(2026, 6, 10, 3, 31, 5, 46, DateTimeKind.Utc).AddTicks(5417), "Chính sách", "tag-policy", "Chính sách hỗ trợ chỉnh trang đô thị, cải tạo vỉa hè Quận 1 năm 2026" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Policies");
        }
    }
}
