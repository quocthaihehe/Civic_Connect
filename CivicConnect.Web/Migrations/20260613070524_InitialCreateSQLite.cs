using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CivicConnect.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateSQLite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DonationCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernmentUnits",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentUnitId = table.Column<string>(type: "TEXT", nullable: true),
                    ProvinceCode = table.Column<string>(type: "TEXT", nullable: false),
                    DistrictCode = table.Column<string>(type: "TEXT", nullable: true),
                    WardCode = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernmentUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernmentUnits_GovernmentUnits_ParentUnitId",
                        column: x => x.ParentUnitId,
                        principalTable: "GovernmentUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    RelatedIssueId = table.Column<string>(type: "TEXT", nullable: true),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Excerpt = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    TagClass = table.Column<string>(type: "TEXT", nullable: false),
                    IssuingUnit = table.Column<string>(type: "TEXT", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DocumentNumber = table.Column<string>(type: "TEXT", nullable: true),
                    DocumentType = table.Column<string>(type: "TEXT", nullable: true),
                    Signer = table.Column<string>(type: "TEXT", nullable: true),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false),
                    CitizenId = table.Column<string>(type: "TEXT", nullable: true),
                    IsPhoneVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    WardCode = table.Column<string>(type: "TEXT", nullable: true),
                    DistrictCode = table.Column<string>(type: "TEXT", nullable: true),
                    ProvinceCode = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GovernmentUnitId = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_GovernmentUnits_GovernmentUnitId",
                        column: x => x.GovernmentUnitId,
                        principalTable: "GovernmentUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UnitCategories",
                columns: table => new
                {
                    GovernmentUnitId = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCategories", x => new { x.GovernmentUnitId, x.Category });
                    table.ForeignKey(
                        name: "FK_UnitCategories_GovernmentUnits_GovernmentUnitId",
                        column: x => x.GovernmentUnitId,
                        principalTable: "GovernmentUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DonationCategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    DonorName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderId = table.Column<string>(type: "TEXT", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    OrderInfo = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    PayUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_DonationCategories_DonationCategoryId",
                        column: x => x.DonationCategoryId,
                        principalTable: "DonationCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    PriorityScore = table.Column<float>(type: "REAL", nullable: false),
                    SeverityScore = table.Column<float>(type: "REAL", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    WardCode = table.Column<string>(type: "TEXT", nullable: false),
                    WardName = table.Column<string>(type: "TEXT", nullable: false),
                    DistrictCode = table.Column<string>(type: "TEXT", nullable: false),
                    DistrictName = table.Column<string>(type: "TEXT", nullable: false),
                    ProvinceCode = table.Column<string>(type: "TEXT", nullable: false),
                    ProvinceName = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "TEXT", nullable: true),
                    AssignedUnitId = table.Column<string>(type: "TEXT", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuthorId = table.Column<string>(type: "TEXT", nullable: false),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    ViewCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ParentIssueId = table.Column<int>(type: "INTEGER", nullable: true),
                    SatisfactionRating = table.Column<int>(type: "INTEGER", nullable: true),
                    SatisfactionComment = table.Column<string>(type: "TEXT", nullable: true),
                    RatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_GovernmentUnits_AssignedUnitId",
                        column: x => x.AssignedUnitId,
                        principalTable: "GovernmentUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Issues_Issues_ParentIssueId",
                        column: x => x.ParentIssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    AuthorId = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ParentCommentId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsOfficialResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsHidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    PublicId = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueImages_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    FromStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ToStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    ChangedById = table.Column<string>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueStatusHistories_AspNetUsers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IssueStatusHistories_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    IssueId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votes_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "Admin", null, "Admin", "ADMIN" },
                    { "Citizen", null, "Citizen", "CITIZEN" },
                    { "DepartmentStaff", null, "DepartmentStaff", "DEPARTMENTSTAFF" },
                    { "OfficialDistrict", null, "OfficialDistrict", "OFFICIALDISTRICT" },
                    { "OfficialProvince", null, "OfficialProvince", "OFFICIALPROVINCE" },
                    { "OfficialWard", null, "OfficialWard", "OFFICIALWARD" }
                });

            migrationBuilder.InsertData(
                table: "DonationCategories",
                columns: new[] { "Id", "CreatedAt", "CurrentAmount", "Description", "IsActive", "Name", "TargetAmount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "Quyên góp mua cây xanh, hoa trang trí trồng tại các tuyến ngõ hẻm, công viên công cộng trên địa bàn Phường Bến Nghé.", true, "Quỹ Trồng Xanh Đô Thị", 50000000m },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "Hỗ trợ lắp đặt hệ thống đèn đường LED thông minh, tiết kiệm điện tại các ngõ hẻm chưa có đủ ánh sáng.", true, "Quỹ Thắp Sáng Ngõ Hẻm", 30000000m },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, "Mua sắm, lắp đặt và sửa chữa các thiết bị vui chơi ngoài trời tại điểm sinh hoạt cộng đồng của phường.", true, "Quỹ Nâng Cấp Sân Chơi Trẻ Em", 100000000m }
                });

            migrationBuilder.InsertData(
                table: "GovernmentUnits",
                columns: new[] { "Id", "Address", "DistrictCode", "Email", "IsActive", "Name", "ParentUnitId", "Phone", "ProvinceCode", "Type", "WardCode" },
                values: new object[,]
                {
                    { "UBND_BIENHOA", "Biên Hòa, Đồng Nai", "297", "bienhoa@dongnai.gov.vn", true, "UBND TP. Biên Hòa", null, "02513822501", "36", 2, null },
                    { "UBND_HOANKIEM", "Hoàn Kiếm, Hà Nội", "001", "hoankiem@hanoi.gov.vn", true, "UBND Quận Hoàn Kiếm", null, "02438252601", "01", 2, null },
                    { "UBND_Q1", "47 Lê Duẩn, Bến Nghé, Quận 1, TP. Hồ Chí Minh", "760", "quan1@tphcm.gov.vn", true, "UBND Quận 1", null, "02838279789", "79", 2, null },
                    { "UBND_THUDAUMOT", "Thủ Dầu Một, Bình Dương", "300", "thudaumot@binhduong.gov.vn", true, "UBND TP. Thủ Dầu Một", null, "02743822001", "37", 2, null }
                });

            migrationBuilder.InsertData(
                table: "Policies",
                columns: new[] { "Id", "Content", "DocumentNumber", "DocumentType", "EffectiveDate", "Excerpt", "IsActive", "IssuingUnit", "PublishedDate", "Signer", "SourceUrl", "Tag", "TagClass", "Title" },
                values: new object[,]
                {
                    { 1, "Nghị định quy định chi tiết các mức xử phạt đối với cá nhân, tổ chức có hành vi vi phạm vệ sinh môi trường đô thị. Mức phạt tiền tối đa đối với cá nhân là 1.000.000đ cho hành vi vứt rác không đúng nơi quy định, 5.000.000đ cho hành vi tự ý đổ rác thải sinh hoạt ra lòng đường, vỉa hè. Các đơn vị kinh doanh lấn chiếm vỉa hè sẽ bị xử phạt từ 10.000.000đ đến 20.000.000đ và buộc khôi phục tình trạng ban đầu.", "45/2026/NĐ-CP", "Nghị định", new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chính phủ ban hành quy định tăng mức phạt đối với các hành vi xả rác bừa bãi, đổ chất thải không đúng nơi quy định và lấn chiếm lòng lề đường.", true, "Chính phủ", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Utc), "Thủ tướng Phạm Minh Chính", "https://vanban.chinhphu.vn", "Nghị định", "tag-law", "Nghị định 45/2026/NĐ-CP về xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị" },
                    { 2, "Bộ Xây dựng ban hành Thông tư hướng dẫn chi tiết quy chuẩn kỹ thuật quốc gia về quy hoạch xây dựng. Trong đó, yêu cầu các khu dân cư mới phải đạt tỷ lệ diện tích cây xanh tối thiểu là 2m2/người, khuyến khích các khu dân cư hiện hữu tận dụng các ngõ hẻm để trồng hoa, cây cảnh công cộng và tạo không gian sinh hoạt cộng đồng tự quản.", "08/2026/TT-BXD", "Thông tư", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bộ Xây dựng ban hành hướng dẫn thực hiện các chỉ tiêu về diện tích cây xanh, hoa và hạ tầng tiện ích tại khu dân cư đô thị.", true, "Bộ Xây dựng", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bộ trưởng Nguyễn Thanh Nghị", "https://moc.gov.vn", "Thông tư", "tag-policy", "Thông tư 08/2026/TT-BXD hướng dẫn về chỉnh trang đô thị và phát triển không gian công cộng xanh" },
                    { 3, "Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung, UBND Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ. Thời gian bắt đầu từ 7:30 sáng ngày Chủ Nhật.", "124/TB-UBND", "Thông báo", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), "UBND Phường phát động lễ ra quân tổng vệ sinh, dọn dẹp rác thải và trang trí tuyến hẻm xanh vào sáng Chủ Nhật.", true, "UBND Phường Bến Nghé", new DateTime(2026, 6, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Chủ tịch UBND Phường", "", "Thông báo", "tag-notice", "Thông báo 124/TB-UBND ra quân tổng vệ sinh môi trường trên địa bàn Phường Bến Nghé" },
                    { 4, "Sáng nay, Quận Đoàn 1 phối hợp với Công ty Môi trường Đô thị tổ chức lễ phát động ra quân làm sạch toàn tuyến kênh chảy qua địa bàn quận. Sau 4 giờ làm việc nỗ lực, lực lượng tình nguyện đã thu gom được hơn 3 tấn rác thải các loại, chủ yếu là túi ni lông, chai nhựa và rác sinh hoạt bị vứt xuống lòng kênh. Đây là hoạt động thường niên nhằm tuyên truyền ý thức bảo vệ môi trường nước cho cư dân xung quanh.", "", "Tin tức", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Chiến dịch 'Chung tay bảo vệ dòng sông quê hương' thu hút hàng trăm bạn trẻ dọn dẹp rác thải nhựa và vớt lục bình.", true, "Quận Đoàn 1", new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "", "https://tuoitre.vn", "Tin tức", "tag-news", "Hơn 500 đoàn viên thanh niên Quận 1 tham gia làm sạch kênh Nhiêu Lộc - Thị Nghè" }
                });

            migrationBuilder.InsertData(
                table: "GovernmentUnits",
                columns: new[] { "Id", "Address", "DistrictCode", "Email", "IsActive", "Name", "ParentUnitId", "Phone", "ProvinceCode", "Type", "WardCode" },
                values: new object[,]
                {
                    { "UBND_BENNGHE", "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TP. Hồ Chí Minh", "760", "bennghe.q1@tphcm.gov.vn", true, "UBND Phường Bến Nghé", "UBND_Q1", "02838290290", "79", 3, "26734" },
                    { "UBND_LONGHUNG", "Long Hưng, Biên Hòa, Đồng Nai", "297", "longhung.bienhoa@dongnai.gov.vn", true, "UBND Phường Long Hưng", "UBND_BIENHOA", "02513822502", "36", 3, "10834" },
                    { "UBND_PHUCUONG", "Phú Cường, Thủ Dầu Một, Bình Dương", "300", "phucuong.tdm@binhduong.gov.vn", true, "UBND Phường Phú Cường", "UBND_THUDAUMOT", "02743822002", "37", 3, "11000" },
                    { "UBND_TRANGTIEN", "Tràng Tiền, Hoàn Kiếm, Hà Nội", "001", "trangtien.hk@hanoi.gov.vn", true, "UBND Phường Tràng Tiền", "UBND_HOANKIEM", "02438252602", "01", 3, "00001" }
                });

            migrationBuilder.InsertData(
                table: "UnitCategories",
                columns: new[] { "Category", "GovernmentUnitId" },
                values: new object[,]
                {
                    { 1, "UBND_BIENHOA" },
                    { 2, "UBND_BIENHOA" },
                    { 3, "UBND_BIENHOA" },
                    { 4, "UBND_BIENHOA" },
                    { 5, "UBND_BIENHOA" },
                    { 1, "UBND_HOANKIEM" },
                    { 2, "UBND_HOANKIEM" },
                    { 3, "UBND_HOANKIEM" },
                    { 4, "UBND_HOANKIEM" },
                    { 5, "UBND_HOANKIEM" },
                    { 1, "UBND_Q1" },
                    { 2, "UBND_Q1" },
                    { 3, "UBND_Q1" },
                    { 4, "UBND_Q1" },
                    { 5, "UBND_Q1" },
                    { 1, "UBND_THUDAUMOT" },
                    { 2, "UBND_THUDAUMOT" },
                    { 3, "UBND_THUDAUMOT" },
                    { 4, "UBND_THUDAUMOT" },
                    { 5, "UBND_THUDAUMOT" },
                    { 1, "UBND_BENNGHE" },
                    { 2, "UBND_BENNGHE" },
                    { 3, "UBND_BENNGHE" },
                    { 4, "UBND_BENNGHE" },
                    { 1, "UBND_LONGHUNG" },
                    { 2, "UBND_LONGHUNG" },
                    { 3, "UBND_LONGHUNG" },
                    { 4, "UBND_LONGHUNG" },
                    { 1, "UBND_PHUCUONG" },
                    { 2, "UBND_PHUCUONG" },
                    { 3, "UBND_PHUCUONG" },
                    { 4, "UBND_PHUCUONG" },
                    { 1, "UBND_TRANGTIEN" },
                    { 2, "UBND_TRANGTIEN" },
                    { 3, "UBND_TRANGTIEN" },
                    { 4, "UBND_TRANGTIEN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GovernmentUnitId",
                table: "AspNetUsers",
                column: "GovernmentUnitId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AuthorId",
                table: "Comments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IssueId",
                table: "Comments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_OrderId",
                table: "Donations",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_UserId",
                table: "Donations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernmentUnits_ParentUnitId",
                table: "GovernmentUnits",
                column: "ParentUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueImages_IssueId",
                table: "IssueImages",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AssignedToUserId",
                table: "Issues",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AssignedUnitId",
                table: "Issues",
                column: "AssignedUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_AuthorId",
                table: "Issues",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_ParentIssueId",
                table: "Issues",
                column: "ParentIssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusHistories_ChangedById",
                table: "IssueStatusHistories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusHistories_IssueId",
                table: "IssueStatusHistories",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_IssueId",
                table: "Votes",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_UserId_IssueId",
                table: "Votes",
                columns: new[] { "UserId", "IssueId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "IssueImages");

            migrationBuilder.DropTable(
                name: "IssueStatusHistories");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "UnitCategories");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "DonationCategories");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GovernmentUnits");
        }
    }
}
