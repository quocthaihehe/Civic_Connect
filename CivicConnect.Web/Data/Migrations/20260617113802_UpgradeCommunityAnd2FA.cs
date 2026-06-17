using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeCommunityAnd2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ForumPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ForumPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PostType",
                table: "ForumPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RelatedIssueId",
                table: "ForumPosts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "ForumPosts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorContact",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabledCustom",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

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
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                column: "SourceUrl",
                value: "https://tuoitre.vn/vut-rac-bua-bai-tieu-bay-se-bi-phat-gap-10-lan-1259236.htm");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                column: "SourceUrl",
                value: "https://tuoitre.vn/de-xuat-xay-cong-vien-khoa-hoc-cho-thieu-nhi-tai-tp-hcm-20240906132157012.htm");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                column: "SourceUrl",
                value: "https://tuoitre.vn/ca-ngan-ban-tre-cung-ba-con-o-binh-hung-hoa-chung-tay-don-rac-trong-cay-nhan-chu-nhat-xanh-20260524125312066.htm");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                column: "SourceUrl",
                value: "https://tuoitre.vn/tp-hcm-chu-tich-phuong-xa-chiu-trach-nhiem-neu-via-he-bi-lan-chiem-20251128174818155.htm");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                column: "SourceUrl",
                value: "https://tuoitre.vn/khanh-hoa-trien-khai-ung-dung-de-dan-phan-anh-hien-truong-tra-cuu-dich-vu-cong-20260415093301147.htm");

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IsActive", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[,]
                {
                    { 7, "<p><strong>LUẬT</strong><br/>\r\n<strong>THỰC HIỆN DÂN CHỦ Ở CƠ SỞ</strong></p>\r\n\r\n<p>Căn cứ Hiến pháp nước Cộng hòa xã hội chủ nghĩa Việt Nam;<br/>\r\nQuốc hội ban hành Luật Thực hiện dân chủ ở cơ sở.</p>\r\n\r\n<p><strong>Chương I: QUY ĐỊNH CHUNG</strong></p>\r\n\r\n<p><strong>Điều 5. Quyền của công dân trong thực hiện dân chủ ở cơ sở</strong><br/>\r\n1. Được thụ hưởng thông tin công khai từ cơ quan nhà nước đầy đủ và minh bạch.<br/>\r\n2. Đề xuất, kiến nghị, phản ánh, thảo luận và tham gia đóng góp ý kiến về các nội dung liên quan trực tiếp đến đời sống xã hội và hạ tầng đô thị tại cơ sở.<br/>\r\n3. Thực hiện quyền kiểm tra, giám sát hoạt động của chính quyền cấp cơ sở theo đúng quy định pháp luật.</p>\r\n\r\n<p><strong>Điều 6. Nghĩa vụ của công dân</strong><br/>\r\n1. Tuân thủ Hiến pháp và pháp luật, tôn trọng trật tự văn minh đô thị.<br/>\r\n2. Phối hợp cùng cơ quan chức năng phản ánh kịp thời các hành vi vi phạm trật tự công cộng, gây ô nhiễm môi trường hoặc hư hỏng hạ tầng kỹ thuật.</p>", "10/2022/QH15", "Luật", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quốc hội ban hành luật quy định về quyền và nghĩa vụ của công dân trong việc giám sát, kiểm tra và phản ánh ý kiến đến chính quyền địa phương.", true, "Quốc hội", new DateTime(2022, 11, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chủ tịch Quốc hội Vương Đình Huệ", "https://chinhphu.vn/", "Luật", "tag-law", "Luật số 10/2022/QH15 về thực hiện dân chủ ở cơ sở" },
                    { 8, "<p><strong>NGHỊ ĐỊNH</strong><br/>\r\n<strong>Quy định về định danh và xác thực điện tử</strong></p>\r\n\r\n<p>Căn cứ Luật Tổ chức Chính phủ ngày 19 tháng 6 năm 2015;<br/>\r\nCăn cứ Luật An toàn thông tin mạng ngày 19 tháng 11 năm 2015;<br/>\r\nTheo đề nghị của Bộ trưởng Bộ Công an;<br/>\r\nChính phủ ban hành Nghị định quy định về định danh và xác thực điện tử.</p>\r\n\r\n<p><strong>Điều 6. Phân loại mức độ tài khoản định danh điện tử</strong><br/>\r\n1. Mức độ 1: Xác thực qua thông tin cá nhân và ảnh chân dung đối chiếu với Cơ sở dữ liệu quốc gia về dân cư.<br/>\r\n2. Mức độ 2: Xác thực mức độ cao nhất tích hợp sinh trắc học vân tay hoặc chíp điện tử CCCD, có giá trị tương đương thẻ căn cước công dân vật lý trong thực hiện thủ tục hành chính.</p>", "59/2022/NĐ-CP", "Nghị định", new DateTime(2022, 10, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Chính phủ quy định về danh tính điện tử, xác thực điện tử và việc sử dụng tài khoản VNeID thay thế giấy tờ vật lý.", true, "Chính phủ", new DateTime(2022, 9, 5, 0, 0, 0, 0, DateTimeKind.Utc), "Thủ tướng Phạm Minh Chính", "https://chinhphu.vn/", "Nghị định", "tag-law", "Nghị định 59/2022/NĐ-CP của Chính phủ quy định về định danh và xác thực điện tử qua VNeID" }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ForumPosts_RelatedIssueId",
                table: "ForumPosts",
                column: "RelatedIssueId");

            migrationBuilder.AddForeignKey(
                name: "FK_ForumPosts_Issues_RelatedIssueId",
                table: "ForumPosts",
                column: "RelatedIssueId",
                principalTable: "Issues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ForumPosts_Issues_RelatedIssueId",
                table: "ForumPosts");

            migrationBuilder.DropIndex(
                name: "IX_ForumPosts_RelatedIssueId",
                table: "ForumPosts");

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "PostType",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "RelatedIssueId",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "ForumPosts");

            migrationBuilder.DropColumn(
                name: "TwoFactorContact",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabledCustom",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TwoFactorType",
                table: "AspNetUsers");

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

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 1,
                column: "SourceUrl",
                value: "https://vanban.chinhphu.vn");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 2,
                column: "SourceUrl",
                value: "https://moc.gov.vn");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 4,
                column: "SourceUrl",
                value: "https://tuoitre.vn");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 5,
                column: "SourceUrl",
                value: "https://tphcm.gov.vn");

            migrationBuilder.UpdateData(
                table: "Policies",
                keyColumn: "Id",
                keyValue: 6,
                column: "SourceUrl",
                value: "https://quan1.hochiminhcity.gov.vn");

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
    }
}
