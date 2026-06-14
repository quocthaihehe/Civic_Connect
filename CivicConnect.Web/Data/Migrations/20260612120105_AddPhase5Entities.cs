using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase5Entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdministrativeProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LegalBasis = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredDocuments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessingTime = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fee = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubmissionPlace = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TemplateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdministrativeProcedures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencyDirectories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    WorkingHours = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Rating = table.Column<float>(type: "real", nullable: false),
                    ReceptionSchedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEmergency = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyDirectories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AdministrativeProcedures",
                columns: new[] { "Id", "Category", "Code", "CreatedAt", "Description", "Fee", "IsActive", "LegalBasis", "ProcessingTime", "RequiredDocuments", "SubmissionPlace", "TemplateUrl", "Title" },
                values: new object[,]
                {
                    { 1, "Hộ tịch", "TTHC-01", new DateTime(2026, 6, 12, 12, 1, 4, 948, DateTimeKind.Utc).AddTicks(6486), "Cán bộ tư pháp xuống tận nhà dân để làm thủ tục đăng ký khai sinh đối với các trường hợp đặc biệt khó khăn, khuyết tật.", "Miễn phí", true, "Luật Hộ tịch 2014, Nghị định 123/2015/NĐ-CP", "1 ngày làm việc", "- Tờ khai đăng ký khai sinh\n- Giấy chứng sinh (hoặc văn bản làm chứng)\n- Thẻ CCCD của người yêu cầu", "Nhà dân / Bộ phận một cửa UBND Phường", "https://dichvucong.gov.vn", "Đăng ký khai sinh lưu động" },
                    { 2, "Hộ tịch", "TTHC-02", new DateTime(2026, 6, 12, 12, 1, 4, 948, DateTimeKind.Utc).AddTicks(6489), "Cấp giấy xác nhận tình trạng hôn nhân để làm thủ tục vay vốn, mua bán đất, hoặc đăng ký kết hôn.", "15.000 VNĐ", true, "Luật Hộ tịch 2014", "3 ngày làm việc", "- Tờ khai yêu cầu xác nhận\n- Thẻ CCCD\n- Quyết định ly hôn (nếu đã từng ly hôn)", "Bộ phận một cửa UBND Phường Bến Nghé", "https://dichvucong.gov.vn", "Xác nhận tình trạng hôn nhân" },
                    { 3, "Hộ tịch", "TTHC-03", new DateTime(2026, 6, 12, 12, 1, 4, 948, DateTimeKind.Utc).AddTicks(6491), "Cấp bản sao trích lục từ sổ gốc hộ tịch (Khai sinh, Kết hôn, Khai tử).", "8.000 VNĐ/bản", true, "Luật Hộ tịch 2014", "Trả kết quả ngay trong ngày", "- Tờ khai yêu cầu cấp bản sao\n- Thẻ CCCD", "Bộ phận một cửa UBND Phường Bến Nghé", "https://dichvucong.gov.vn", "Cấp bản sao trích lục hộ tịch" }
                });

            migrationBuilder.InsertData(
                table: "AgencyDirectories",
                columns: new[] { "Id", "Address", "Email", "IsEmergency", "Latitude", "Longitude", "Name", "OrderIndex", "Phone", "Rating", "ReceptionSchedule", "Type", "WorkingHours" },
                values: new object[,]
                {
                    { 1, "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TPHCM", "congan.bennghe@tphcm.gov.vn", true, 10.7811, 106.7051, "Công an Phường Bến Nghé", 1, "02838297335", 4.8f, "Tiếp công dân hằng ngày vào giờ hành chính (Trưởng công an phường tiếp vào sáng Thứ 5).", "Công an", "Trực 24/7" },
                    { 2, "Số 1 Lý Tự Trọng, Bến Nghé, Quận 1, TPHCM", "tramyte.bennghe@tphcm.gov.vn", false, 10.7788, 106.7032, "Trạm Y tế Phường Bến Nghé", 2, "02838222956", 4.5f, "Tiêm chủng định kỳ vào sáng Thứ 4 và Thứ 6 hằng tuần.", "Y tế", "07:30 - 16:30 (Thứ 2 - Thứ 6)" },
                    { 3, "194 Pasteur, Phường Võ Thị Sáu, Quận 3, TPHCM", "cskh@capnuocbenthanh.com", false, 10.782999999999999, 106.694, "Công ty Cấp nước Bến Thành", 3, "19001224", 4f, "Tiếp nhận hồ sơ lắp đặt mới tại quầy vào giờ hành chính.", "Hạ tầng", "08:00 - 17:00 (Thứ 2 - Thứ 6)" },
                    { 4, "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TPHCM", "bennghe.q1@tphcm.gov.vn", false, 10.7811, 106.7051, "Lịch tiếp dân Chủ tịch UBND Phường Bến Nghé", 0, "02838290290", 5f, "Đồng chí Chủ tịch UBND Phường tiếp công dân định kỳ để giải quyết khiếu nại, tố cáo và các vấn đề dân sinh phức tạp.", "Hành chính", "Sáng Thứ 3 & Thứ 5 hằng tuần" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdministrativeProcedures");

            migrationBuilder.DropTable(
                name: "AgencyDirectories");
        }
    }
}
