using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMorePoliciesSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9492));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9495));

            migrationBuilder.UpdateData(
                table: "AdministrativeProcedures",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9497));

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IsActive", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[,]
                {
                    { 5, "<p><strong>ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH</strong><br/>\r\nSố: 18/2026/QĐ-UBND<br/>\r\n<em>TP. Hồ Chí Minh, ngày 15 tháng 05 năm 2026</em></p>\r\n\r\n<p><strong>QUYẾT ĐỊNH</strong><br/>\r\n<strong>Ban hành Quy định về quản lý và sử dụng tạm thời một phần lòng đường, hè phố trên địa bàn Thành phố Hồ Chí Minh</strong></p>\r\n\r\n<p>Căn cứ Luật Tổ chức chính quyền địa phương ngày 19 tháng 6 năm 2015;<br/>\r\nCăn cứ Luật Giao thông đường bộ ngày 13 tháng 11 năm 2008;<br/>\r\nNhằm thiết lập trật tự, kỷ cương đô thị, đồng thời giải quyết nhu cầu sử dụng tạm thời hè phố của người dân và doanh nghiệp một cách công khai, minh bạch;<br/>\r\nTheo đề nghị của Giám đốc Sở Giao thông vận tải Thành phố Hồ Chí Minh.</p>\r\n\r\n<p><strong>Điều 1. Phạm vi và nguyên tắc sử dụng tạm thời hè phố</strong><br/>\r\n1. Hè phố chỉ được sử dụng tạm thời cho mục đích ngoài giao thông khi phần hè phố còn lại dành cho người đi bộ có bề rộng tối thiểu là 1,5 mét, thông suốt và an toàn.<br/>\r\n2. Việc sử dụng tạm thời phải được cấp phép bởi cơ quan có thẩm quyền và phải đóng phí sử dụng đường bộ theo quy định.</p>\r\n\r\n<p><strong>Điều 2. Các trường hợp được sử dụng tạm thời đóng phí</strong><br/>\r\n1. Điểm tổ chức kinh doanh dịch vụ mua, bán hàng hóa, ẩm thực tại các tuyến phố đi bộ hoặc khu vực được quy hoạch.<br/>\r\n2. Điểm trông giữ xe đạp, xe máy, xe ô tô có thu tiền dịch vụ.<br/>\r\n3. Tổ chức các hoạt động văn hóa, xã hội, tuyên truyền cổ động lớn của Thành phố hoặc Quận/Huyện.</p>\r\n\r\n<p><strong>Điều 3. Thẩm quyền cấp phép và mức phí</strong><br/>\r\n1. Ủy ban nhân dân các Quận, Huyện và Thành phố Thủ Đức thực hiện cấp phép sử dụng tạm thời hè phố thuộc phạm vi quản lý hành chính.<br/>\r\n2. Mức phí được áp dụng theo biểu giá phân chia theo 5 khu vực đô thị của Thành phố, dao động từ 20.000 đồng đến 350.000 đồng/m²/tháng đối với kinh doanh hàng hóa và trông giữ xe.</p>", "18/2026/QĐ-UBND", "Quyết định", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "UBND Thành phố Hồ Chí Minh ban hành quy định mới về quản lý, cấp phép và thu phí sử dụng tạm thời một phần lòng đường, hè phố không vì mục đích giao thông.", true, "UBND TP.HCM", new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Chủ tịch Phan Văn Mãi", "https://tphcm.gov.vn", "Quyết định", "tag-law", "Quyết định 18/2026/QĐ-UBND về quản lý và sử dụng tạm thời một phần lòng đường, vỉa hè tại TP.HCM" },
                    { 6, "<p><strong>ỦY BAN NHÂN DÂN QUẬN 1</strong><br/>\r\nSố: 89/KH-UBND<br/>\r\n<em>Quận 1, ngày 25 tháng 05 năm 2026</em></p>\r\n\r\n<p><strong>KẾ HOẠCH</strong><br/>\r\n<strong>Nâng cao hiệu quả công tác tiếp nhận, xử lý và trả lời phản ánh, kiến nghị của người dân qua Hệ thống tương tác chính quyền số</strong></p>\r\n\r\n<p>Nhằm tăng cường sự hài lòng của người dân, rút ngắn thời gian xử lý các sự cố về hạ tầng đô thị, an ninh trật tự và vệ sinh môi trường trên địa bàn Quận 1;<br/>\r\nỦy ban nhân dân Quận 1 ban hành Kế hoạch hành động cụ thể cho giai đoạn 2026 - 2027.</p>\r\n\r\n<p><strong>1. Chỉ tiêu xử lý phản ánh kiến nghị</strong><br/>\r\n- 100% phản ánh của người dân về các sự cố khẩn cấp (như sụt lún đường, đứt cáp điện, ô nhiễm nghiêm trọng) phải được tiếp nhận và xử lý ban đầu trong vòng 2 giờ.<br/>\r\n- Ít nhất 95% phản ánh thông thường (như rác thải sinh hoạt, lấn chiếm hè phố, tiếng ồn khu dân cư) phải được giải quyết dứt điểm và phản hồi kết quả cho công dân trong vòng 24 giờ kể từ khi tiếp nhận.</p>\r\n\r\n<p><strong>2. Phân công trách nhiệm</strong><br/>\r\n- Ủy ban nhân dân 10 phường trực thuộc chịu trách nhiệm xử lý trực tiếp tại hiện trường đối với các phản ánh về trật tự đô thị, vệ sinh môi trường trên địa bàn.<br/>\r\n- Phòng Quản lý đô thị phối hợp cùng Phòng Tài nguyên và Môi trường Quận giám sát, đôn đốc tiến độ xử lý và hậu kiểm kết quả tại các đơn vị cơ sở.</p>\r\n\r\n<p><strong>3. Khen thưởng và kỷ luật</strong><br/>\r\n- Đưa chỉ tiêu tỷ lệ giải quyết đúng hạn các phản ánh của công dân làm tiêu chí xếp loại thi đua hàng năm của các đơn vị phường, phòng ban và cá nhân người đứng đầu.<br/>\r\n- Xử lý nghiêm khắc đối với các trường hợp trễ hẹn không có lý do chính đáng hoặc trả lời phản ánh mang tính đối phó, không dứt điểm.</p>", "89/KH-UBND", "Kế hoạch", new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Utc), "UBND Quận 1 ban hành kế hoạch hành động nhằm tối ưu hóa quy trình tiếp nhận phản ánh về trật tự đô thị, vệ sinh môi trường và hạ tầng kỹ thuật.", true, "UBND Quận 1", new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Utc), "Chủ tịch UBND Quận 1", "https://quan1.hochiminhcity.gov.vn", "Kế hoạch", "tag-policy", "Kế hoạch 89/KH-UBND nâng cao hiệu quả tiếp nhận và giải quyết phản ánh, kiến nghị của người dân đô thị" }
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "MaintenanceMode",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9432));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "OrganizationName",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9434));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "SystemLogoUrl",
                column: "UpdatedAt",
                value: new DateTime(2026, 6, 14, 8, 42, 19, 34, DateTimeKind.Utc).AddTicks(9434));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6);

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
        }
    }
}
