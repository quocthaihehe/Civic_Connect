using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyDetailsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Policies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signer",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "Title" },
                values: new object[] { "Nghị định quy định chi tiết các mức xử phạt đối với cá nhân, tổ chức có hành vi vi phạm vệ sinh môi trường đô thị. Mức phạt tiền tối đa đối với cá nhân là 1.000.000đ cho hành vi vứt rác không đúng nơi quy định, 5.000.000đ cho hành vi tự ý đổ rác thải sinh hoạt ra lòng đường, vỉa hè. Các đơn vị kinh doanh lấn chiếm vỉa hè sẽ bị xử phạt từ 10.000.000đ đến 20.000.000đ và buộc khôi phục tình trạng ban đầu.", "45/2026/NĐ-CP", "Nghị định", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chính phủ ban hành quy định tăng mức phạt đối với các hành vi xả rác bừa bãi, đổ chất thải không đúng nơi quy định và lấn chiếm lòng lề đường.", "Chính phủ", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Thủ tướng Phạm Minh Chính", "https://vanban.chinhphu.vn", "Nghị định", "Nghị định 45/2026/NĐ-CP về xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị" });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[] { "Bộ Xây dựng ban hành Thông tư hướng dẫn chi tiết quy chuẩn kỹ thuật quốc gia về quy hoạch xây dựng. Trong đó, yêu cầu các khu dân cư mới phải đạt tỷ lệ diện tích cây xanh tối thiểu là 2m2/người, khuyến khích các khu dân cư hiện hữu tận dụng các ngõ hẻm để trồng hoa, cây cảnh công cộng và tạo không gian sinh hoạt cộng đồng tự quản.", "08/2026/TT-BXD", "Thông tư", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bộ Xây dựng ban hành hướng dẫn thực hiện các chỉ tiêu về diện tích cây xanh, hoa và hạ tầng tiện ích tại khu dân cư đô thị.", "Bộ Xây dựng", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bộ trưởng Nguyễn Thanh Nghị", "https://moc.gov.vn", "Thông tư", "tag-policy", "Thông tư 08/2026/TT-BXD hướng dẫn về chỉnh trang đô thị và phát triển không gian công cộng xanh" });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[] { "Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung, UBND Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ. Thời gian bắt đầu từ 7:30 sáng ngày Chủ Nhật.", "124/TB-UBND", "Thông báo", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), "UBND Phường phát động lễ ra quân tổng vệ sinh, dọn dẹp rác thải và trang trí tuyến hẻm xanh vào sáng Chủ Nhật.", "UBND Phường Bến Nghé", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chủ tịch UBND Phường", "", "Thông báo", "tag-notice", "Thông báo 124/TB-UBND ra quân tổng vệ sinh môi trường trên địa bàn Phường Bến Nghé" });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IsActive", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[] { 4, "Sáng nay, Quận Đoàn 1 phối hợp với Công ty Môi trường Đô thị tổ chức lễ phát động ra quân làm sạch toàn tuyến kênh chảy qua địa bàn quận. Sau 4 giờ làm việc nỗ lực, lực lượng tình nguyện đã thu gom được hơn 3 tấn rác thải các loại, chủ yếu là túi ni lông, chai nhựa và rác sinh hoạt bị vứt xuống lòng kênh. Đây là hoạt động thường niên nhằm tuyên truyền ý thức bảo vệ môi trường nước cho cư dân xung quanh.", "", "Tin tức", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Chiến dịch 'Chung tay bảo vệ dòng sông quê hương' thu hút hàng trăm bạn trẻ dọn dẹp rác thải nhựa và vớt lục bình.", true, "Quận Đoàn 1", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "", "https://tuoitre.vn", "Tin tức", "tag-news", "Hơn 500 đoàn viên thanh niên Quận 1 tham gia làm sạch kênh Nhiêu Lộc - Thị Nghè" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "Signer",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "Policies");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Content", "Excerpt", "IssuingUnit", "PublishedDate", "Tag", "Title" },
                values: new object[] { "Nội dung chi tiết nghị định mới...", "Chính phủ vừa ban hành nghị định mới tăng mức phạt đối với các hành vi xả rác bừa bãi và lấn chiếm lòng lề đường tại đô thị.", "UBND Quận 1", new DateTime(2026, 6, 8, 7, 17, 48, 310, DateTimeKind.Utc).AddTicks(8145), "Luật mới", "Nghị định mới về xử phạt hành chính vi phạm môi trường đô thị" });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Content", "Excerpt", "IssuingUnit", "PublishedDate", "Tag", "TagClass", "Title" },
                values: new object[] { "Nội dung thông báo chi tiết...", "Thực hiện nếp sống văn minh đô thị, UBND Phường tổ chức đợt ra quân tổng vệ sinh các tuyến đường trọng điểm vào sáng Chủ Nhật tuần này.", "UBND Phường Bến Nghé", new DateTime(2026, 6, 9, 7, 17, 48, 310, DateTimeKind.Utc).AddTicks(8350), "Thông báo", "tag-notice", "Thông báo ra quân dọn dẹp vệ sinh môi trường trên địa bàn Phường Bến Nghé" });

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Content", "Excerpt", "IssuingUnit", "PublishedDate", "Tag", "TagClass", "Title" },
                values: new object[] { "Nội dung chính sách chi tiết...", "Chương trình nâng cấp, chỉnh trang các tuyến vỉa hè trung tâm nhằm nâng cao mỹ quan đô thị và tạo không gian đi bộ an toàn cho người dân.", "UBND Quận 1", new DateTime(2026, 6, 10, 7, 17, 48, 310, DateTimeKind.Utc).AddTicks(8353), "Chính sách", "tag-policy", "Chính sách hỗ trợ chỉnh trang đô thị, cải tạo vỉa hè Quận 1 năm 2026" });
        }
    }
}
