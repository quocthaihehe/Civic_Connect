using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPolicyAiSummaryTableAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyAiSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    ShortSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BulletPointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RealWorldExample = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyAiSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyAiSummaries_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(634));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(639));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(642));

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "<p><strong>NGHỊ ĐỊNH</strong><br/>\r\n<strong>Quy định xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị</strong></p>\r\n\r\n<p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015; Luật sửa đổi, bổ sung một số điều của Luật Tổ chức Chính phủ và Luật Tổ chức chính quyền địa phương ngày 22 tháng 11 năm 2019;<br/>\r\nCăn cứ Luật Bảo vệ môi trường ngày 17 tháng 11 năm 2020;<br/>\r\nCăn cứ Luật Xử lý vi phạm hành chính ngày 20 tháng 6 năm 2012; Luật sửa đổi, bổ sung một số điều của Luật Xử lý vi phạm hành chính ngày 13 tháng 11 năm 2020;<br/>\r\nTheo đề nghị của Bộ trưởng Bộ Tài nguyên và Môi trường;<br/>\r\nChính phủ ban hành Nghị định quy định xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị.</p>\r\n\r\n<p><strong>Chương I: QUY ĐỊNH CHUNG</strong></p>\r\n\r\n<p><strong>Điều 1. Phạm vi điều chỉnh</strong><br/>\r\nNghị định này quy định các hành vi vi phạm hành chính, hình thức xử phạt, mức xử phạt, biện pháp khắc phục hậu quả đối với hành vi vi phạm pháp luật về bảo vệ môi trường tại khu vực đô thị, khu dân cư tập trung, nơi công cộng.</p>\r\n\r\n<p><strong>Điều 2. Đối tượng áp dụng</strong><br/>\r\n1. Cá nhân, tổ chức trong nước và nước ngoài có hành vi vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị trên lãnh thổ nước Cộng hòa xã hội chủ nghĩa Việt Nam.<br/>\r\n2. Cơ quan có thẩm quyền xử phạt và cá nhân, tổ chức liên quan.</p>\r\n\r\n<p><strong>Chương II: HÀNH VI VI PHẠM, HÌNH THỨC VÀ MỨC XỬ PHẠT</strong></p>\r\n\r\n<p><strong>Điều 10. Vi phạm quy định về bảo vệ môi trường nơi công cộng, khu đô thị, khu dân cư</strong><br/>\r\n1. Phạt tiền từ 1.000.000 đồng đến 2.000.000 đồng đối với hành vi vứt, thải, bỏ đầu, mẩu, tàn thuốc lá không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng.<br/>\r\n2. Phạt tiền từ 3.000.000 đồng đến 5.000.000 đồng đối với hành vi vệ sinh cá nhân (tiểu tiện, đại tiện) không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng.<br/>\r\n3. Phạt tiền từ 5.000.000 đồng đến 10.000.000 đồng đối với hành vi vứt, thải, bỏ rác thải sinh hoạt, đổ nước thải không đúng nơi quy định tại khu chung cư, thương mại, dịch vụ hoặc nơi công cộng, trừ các hành vi quy định tại khoản 4 Điều này.<br/>\r\n4. Phạt tiền từ 10.000.000 đồng đến 15.000.000 đồng đối với hành vi vứt, thải rác thải sinh hoạt trên vỉa hè, lòng đường hoặc vào hệ thống thoát nước mưa, nước thải đô thị; đổ nước thải không đúng quy định trên vỉa hè, lòng đường phố.</p>\r\n\r\n<p><strong>Điều 11. Vi phạm về lấn chiếm lòng lề đường, xả rác thải xây dựng</strong><br/>\r\n1. Phạt tiền từ 20.000.000 đồng đến 30.000.000 đồng đối với hành vi đổ, bỏ chất thải rắn xây dựng, đất đá, phế liệu xây dựng trái phép ra môi trường hoặc lấn chiếm lòng lề đường, hè phố đô thị.<br/>\r\n2. Biện pháp khắc phục hậu quả: Buộc khôi phục lại tình trạng ban đầu; buộc vận chuyển chất thải, phế liệu xây dựng đến điểm tập kết đúng quy định.</p>\r\n\r\n<p><strong>Chương III: THẨM QUYỀN VÀ THỦ TỤC XỬ PHẠT</strong></p>\r\n\r\n<p><strong>Điều 25. Thẩm quyền của Chủ tịch Ủy ban nhân dân các cấp</strong><br/>\r\n1. Chủ tịch Ủy ban nhân dân cấp xã có quyền phạt cảnh cáo, phạt tiền đến 5.000.000 đồng, tịch thu tang vật vi phạm.<br/>\r\n2. Chủ tịch Ủy ban nhân dân cấp huyện có quyền phạt tiền đến 50.000.000 đồng, đình chỉ hoạt động gây ô nhiễm môi trường.<br/>\r\n3. Chủ tịch Ủy ban nhân dân cấp tỉnh có quyền phạt tiền đến 100.000.000 đồng đối với cá nhân và 200.000.000 đồng đối với tổ chức.</p>");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                column: "Content",
                value: "<p><strong>BỘ XÂY DỰNG</strong><br/>\r\nSố: 08/2026/TT-BXD<br/>\r\n<em>Hà Nội, ngày 01 tháng 06 năm 2026</em></p>\r\n\r\n<p><strong>THÔNG TƯ</strong><br/>\r\n<strong>Hướng dẫn về chỉnh trang đô thị, cải tạo và đồng bộ hóa không gian công cộng xanh</strong></p>\r\n\r\n<p>Căn cứ Luật Xây dựng ngày 18 tháng 6 năm 2014 và Luật sửa đổi, bổ sung một số điều của Luật Xây dựng ngày 17 tháng 6 năm 2020;<br/>\r\nCăn cứ Luật Quy hoạch đô thị ngày 17 tháng 6 năm 2009;<br/>\r\nNhằm nâng cao chất lượng hạ tầng xanh và phát triển môi trường sống trong lành tại các đô thị loại I và đặc biệt;<br/>\r\nBộ trưởng Bộ Xây dựng ban hành Thông tư hướng dẫn về chỉnh trang đô thị, cải tạo và phát triển không gian công cộng xanh.</p>\r\n\r\n<p><strong>Điều 1. Phạm vi áp dụng và tiêu chuẩn xanh</strong><br/>\r\n1. Tiêu chuẩn diện tích cây xanh: Đề xuất các khu đô thị mới phải đạt tối thiểu 2m² cây xanh/người dân. Các khu dân cư hiện hữu tận dụng tối đa vỉa hè, các tuyến hẻm để trồng hoa, cây xanh công cộng tự quản.<br/>\r\n2. Hạ tầng kỹ thuật vỉa hè: Khuyến khích cải tạo, chỉnh trang đồng bộ bằng đá tự nhiên có độ bền cao, thiết lập các gờ nổi dẫn đường cho người khiếm thị.</p>\r\n\r\n<p><strong>Điều 2. Quy chuẩn cải tạo và nguồn lực hỗ trợ</strong><br/>\r\n1. Ưu tiên ngân sách hỗ trợ 70% tổng kinh phí đầu tư xây dựng cơ sở hạ tầng xanh. Vận động nguồn lực xã hội đóng góp 30% từ các doanh nghiệp, hộ gia đình mặt tiền đường.<br/>\r\n2. Hỗ trợ 100% kinh phí chỉnh trang kết nối từ nhà ra vỉa hè cho các hộ nghèo, cận nghèo và gia đình chính sách.</p>");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                column: "Content",
                value: "<p><strong>ỦY BAN NHÂN DÂN PHƯỜNG BẾN NGHÉ</strong><br/>\r\nSố: 124/TB-UBND<br/>\r\n<em>Bến Nghé, ngày 10 tháng 06 năm 2026</em></p>\r\n\r\n<p><strong>THÔNG BÁO</strong><br/>\r\n<strong>Về việc tổ chức ra quân tổng vệ sinh môi trường, xóa quảng cáo rao vặt trái phép và chỉnh trang mỹ quan đô thị</strong></p>\r\n\r\n<p>Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung trên địa bàn Phường, Ủy ban nhân dân Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ.</p>\r\n\r\n<p><strong>1. Thời gian ra quân:</strong><br/>\r\nBắt đầu từ 07 giờ 30 phút sáng Chủ Nhật ngày 21 tháng 06 năm 2026.</p>\r\n\r\n<p><strong>2. Phân công nhiệm vụ cụ thể:</strong><br/>\r\n- Đoàn Thanh niên phường phối hợp dọn dẹp rác thải nhựa dọc các tuyến đường lớn như Nguyễn Huệ và Lê Lợi.<br/>\r\n- Ban điều hành khu phố vận động từng hộ gia đình quét dọn sạch sẽ, phân loại rác thải tại nguồn trước cửa nhà mình.<br/>\r\n- Lực lượng chức năng phối hợp bóc gỡ quảng cáo vẽ bậy trái phép trên tường, tủ điện công cộng.</p>");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(566));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(568));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 7, 51, 26, 619, DateTimeKind.Utc).AddTicks(569));

            migrationBuilder.CreateIndex(
                name: "IX_PolicyAiSummaries_PolicyId_IsActive",
                table: "PolicyAiSummaries",
                columns: new[] { "PolicyId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyAiSummaries");

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9798));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9801));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9803));

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                column: "Content",
                value: "Nghị định quy định chi tiết các mức xử phạt đối với cá nhân, tổ chức có hành vi vi phạm vệ sinh môi trường đô thị. Mức phạt tiền tối đa đối với cá nhân là 1.000.000đ cho hành vi vứt rác không đúng nơi quy định, 5.000.000đ cho hành vi tự ý đổ rác thải sinh hoạt ra lòng đường, vỉa hè. Các đơn vị kinh doanh lấn chiếm vỉa hè sẽ bị xử phạt từ 10.000.000đ đến 20.000.000đ và buộc khôi phục tình trạng ban đầu.");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                column: "Content",
                value: "Bộ Xây dựng ban hành Thông tư hướng dẫn chi tiết quy chuẩn kỹ thuật quốc gia về quy hoạch xây dựng. Trong đó, yêu cầu các khu dân cư mới phải đạt tỷ lệ diện tích cây xanh tối thiểu là 2m2/người, khuyến khích các khu dân cư hiện hữu tận dụng các ngõ hẻm để trồng hoa, cây cảnh công cộng và tạo không gian sinh hoạt cộng đồng tự quản.");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 3,
                column: "Content",
                value: "Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung, UBND Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ. Thời gian bắt đầu từ 7:30 sáng ngày Chủ Nhật.");

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9733));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9735));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 12, 13, 38, 0, 588, DateTimeKind.Utc).AddTicks(9735));
        }
    }
}
