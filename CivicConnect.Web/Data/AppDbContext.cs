using CivicConnect.Web.Models.Entities;
using CivicConnect.Web.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace CivicConnect.Web.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueImage> IssueImages { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<IssueStatusHistory> IssueStatusHistories { get; set; }
        public DbSet<GovernmentUnit> GovernmentUnits { get; set; }
        public DbSet<UnitCategory> UnitCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<DonationCategory> DonationCategories { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<SmartRoutingRule> SmartRoutingRules { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<AdministrativeProcedure> AdministrativeProcedures { get; set; }
        public DbSet<AgencyDirectory> AgencyDirectories { get; set; }
        public DbSet<ForumPost> ForumPosts { get; set; }
        public DbSet<ForumComment> ForumComments { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<Petition> Petitions { get; set; }
        public DbSet<PetitionSignature> PetitionSignatures { get; set; }
        public DbSet<CommunityEvent> CommunityEvents { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Community Entities
            modelBuilder.Entity<ForumPost>().HasOne(p => p.Author).WithMany().HasForeignKey(p => p.AuthorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ForumComment>().HasOne(c => c.Author).WithMany().HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<PollVote>().HasIndex(v => new { v.UserId, v.PollId }).IsUnique();
            modelBuilder.Entity<PollVote>().HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PollVote>().HasOne(v => v.Poll).WithMany(p => p.Votes).HasForeignKey(v => v.PollId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<PetitionSignature>().HasIndex(s => new { s.UserId, s.PetitionId }).IsUnique();
            modelBuilder.Entity<PetitionSignature>().HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<EventRegistration>().HasIndex(r => new { r.UserId, r.EventId }).IsUnique();
            modelBuilder.Entity<EventRegistration>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);

            // Cấu hình bảng biểu quyết Vote
            modelBuilder.Entity<Vote>(entity =>
            {
                // Một user chỉ vote tối đa 1 lần cho mỗi issue (up hoặc down)
                entity.HasIndex(v => new { v.UserId, v.IssueId }).IsUnique();
                
                // Restrict cascade delete từ User
                entity.HasOne<ApplicationUser>()
                    .WithMany(u => u.Votes)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Cascade delete từ Issue
                entity.HasOne<Issue>()
                    .WithMany(i => i.Votes)
                    .HasForeignKey(v => v.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình bảng phản ánh Issue
            modelBuilder.Entity<Issue>(entity =>
            {
                // Author relationship - Restrict cascade
                entity.HasOne(i => i.Author)
                    .WithMany(u => u.Issues)
                    .HasForeignKey(i => i.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // AssignedTo relationship - Restrict cascade
                entity.HasOne(i => i.AssignedTo)
                    .WithMany()
                    .HasForeignKey(i => i.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // AssignedUnit relationship - Restrict cascade
                entity.HasOne(i => i.AssignedUnit)
                    .WithMany(gu => gu.AssignedIssues)
                    .HasForeignKey(i => i.AssignedUnitId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Tự liên kết để Escalation (Chuyển cấp)
                entity.HasOne(i => i.ParentIssue)
                    .WithMany(i => i.ChildIssues)
                    .HasForeignKey(i => i.ParentIssueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình bình luận Comment
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Issue)
                    .WithMany(i => i.Comments)
                    .HasForeignKey(c => c.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình lịch sử trạng thái IssueStatusHistory
            modelBuilder.Entity<IssueStatusHistory>(entity =>
            {
                entity.HasOne(h => h.ChangedBy)
                    .WithMany()
                    .HasForeignKey(h => h.ChangedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Issue)
                    .WithMany(i => i.StatusHistory)
                    .HasForeignKey(h => h.IssueId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình đơn vị cơ quan GovernmentUnit
            modelBuilder.Entity<GovernmentUnit>(entity =>
            {
                entity.HasOne(gu => gu.ParentUnit)
                    .WithMany(gu => gu.ChildUnits)
                    .HasForeignKey(gu => gu.ParentUnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình bảng phụ thẩm quyền UnitCategory
            modelBuilder.Entity<UnitCategory>(entity =>
            {
                entity.HasKey(uc => new { uc.GovernmentUnitId, uc.Category });

                entity.HasOne(uc => uc.GovernmentUnit)
                    .WithMany(gu => gu.Categories)
                    .HasForeignKey(uc => uc.GovernmentUnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed các vai trò Identity (Roles)
            var roles = new[] { "Admin", "OfficialProvince", "OfficialDistrict", "OfficialWard", "DepartmentStaff", "Citizen" };
            foreach (var roleName in roles)
            {
                modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
                {
                    Id = roleName,
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }

            // Seed đơn vị cơ quan hành chính Quận 1 và Phường Bến Nghé (Mã chuẩn VN)
            modelBuilder.Entity<GovernmentUnit>().HasData(
                new GovernmentUnit
                {
                    Id = "UBND_Q1",
                    Name = "UBND Quận 1",
                    Type = GovernmentUnitType.District,
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    Address = "47 Lê Duẩn, Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    Phone = "02838279789",
                    Email = "quan1@tphcm.gov.vn"
                },
                new GovernmentUnit
                {
                    Id = "UBND_BENNGHE",
                    Name = "UBND Phường Bến Nghé",
                    Type = GovernmentUnitType.Ward,
                    ParentUnitId = "UBND_Q1",
                    ProvinceCode = "79",
                    DistrictCode = "760",
                    WardCode = "26734",
                    Address = "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    Phone = "02838290290",
                    Email = "bennghe.q1@tphcm.gov.vn"
                }
            );

            // Seed thẩm quyền cơ quan xử lý theo danh mục
            modelBuilder.Entity<UnitCategory>().HasData(
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Traffic },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Environment },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Security },
                new UnitCategory { GovernmentUnitId = "UBND_BENNGHE", Category = IssueCategory.Infrastructure },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Traffic },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Environment },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Security },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Infrastructure },
                new UnitCategory { GovernmentUnitId = "UBND_Q1", Category = IssueCategory.Administration }
            );

            // Seed sample policies/announcements with legal metadata
            modelBuilder.Entity<Policy>().HasData(
                new Policy
                {
                    Id = 1,
                    Title = "Nghị định 45/2026/NĐ-CP về xử phạt vi phạm hành chính trong lĩnh vực bảo vệ môi trường đô thị",
                    Excerpt = "Chính phủ ban hành quy định tăng mức phạt đối với các hành vi xả rác bừa bãi, đổ chất thải không đúng nơi quy định và lấn chiếm lòng lề đường.",
                    Content = "Nghị định quy định chi tiết các mức xử phạt đối với cá nhân, tổ chức có hành vi vi phạm vệ sinh môi trường đô thị. Mức phạt tiền tối đa đối với cá nhân là 1.000.000đ cho hành vi vứt rác không đúng nơi quy định, 5.000.000đ cho hành vi tự ý đổ rác thải sinh hoạt ra lòng đường, vỉa hè. Các đơn vị kinh doanh lấn chiếm vỉa hè sẽ bị xử phạt từ 10.000.000đ đến 20.000.000đ và buộc khôi phục tình trạng ban đầu.",
                    Tag = "Nghị định",
                    TagClass = "tag-law",
                    IssuingUnit = "Chính phủ",
                    PublishedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "45/2026/NĐ-CP",
                    DocumentType = "Nghị định",
                    Signer = "Thủ tướng Phạm Minh Chính",
                    SourceUrl = "https://vanban.chinhphu.vn",
                    EffectiveDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 2,
                    Title = "Thông tư 08/2026/TT-BXD hướng dẫn về chỉnh trang đô thị và phát triển không gian công cộng xanh",
                    Excerpt = "Bộ Xây dựng ban hành hướng dẫn thực hiện các chỉ tiêu về diện tích cây xanh, hoa và hạ tầng tiện ích tại khu dân cư đô thị.",
                    Content = "Bộ Xây dựng ban hành Thông tư hướng dẫn chi tiết quy chuẩn kỹ thuật quốc gia về quy hoạch xây dựng. Trong đó, yêu cầu các khu dân cư mới phải đạt tỷ lệ diện tích cây xanh tối thiểu là 2m2/người, khuyến khích các khu dân cư hiện hữu tận dụng các ngõ hẻm để trồng hoa, cây cảnh công cộng và tạo không gian sinh hoạt cộng đồng tự quản.",
                    Tag = "Thông tư",
                    TagClass = "tag-policy",
                    IssuingUnit = "Bộ Xây dựng",
                    PublishedDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "08/2026/TT-BXD",
                    DocumentType = "Thông tư",
                    Signer = "Bộ trưởng Nguyễn Thanh Nghị",
                    SourceUrl = "https://moc.gov.vn",
                    EffectiveDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 3,
                    Title = "Thông báo 124/TB-UBND ra quân tổng vệ sinh môi trường trên địa bàn Phường Bến Nghé",
                    Excerpt = "UBND Phường phát động lễ ra quân tổng vệ sinh, dọn dẹp rác thải và trang trí tuyến hẻm xanh vào sáng Chủ Nhật.",
                    Content = "Nhằm xây dựng nếp sống văn minh đô thị và giữ gìn vệ sinh chung, UBND Phường Bến Nghé phát động chiến dịch ra quân quét dọn các tuyến đường chính và chỉnh trang cây xanh tại ngõ hẻm 45 Lê Thánh Tôn. Kính mời toàn thể nhân dân, các ban ngành đoàn thể và tổ dân phố tham gia đầy đủ. Thời gian bắt đầu từ 7:30 sáng ngày Chủ Nhật.",
                    Tag = "Thông báo",
                    TagClass = "tag-notice",
                    IssuingUnit = "UBND Phường Bến Nghé",
                    PublishedDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "124/TB-UBND",
                    DocumentType = "Thông báo",
                    Signer = "Chủ tịch UBND Phường",
                    SourceUrl = "",
                    EffectiveDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new Policy
                {
                    Id = 4,
                    Title = "Hơn 500 đoàn viên thanh niên Quận 1 tham gia làm sạch kênh Nhiêu Lộc - Thị Nghè",
                    Excerpt = "Chiến dịch 'Chung tay bảo vệ dòng sông quê hương' thu hút hàng trăm bạn trẻ dọn dẹp rác thải nhựa và vớt lục bình.",
                    Content = "Sáng nay, Quận Đoàn 1 phối hợp với Công ty Môi trường Đô thị tổ chức lễ phát động ra quân làm sạch toàn tuyến kênh chảy qua địa bàn quận. Sau 4 giờ làm việc nỗ lực, lực lượng tình nguyện đã thu gom được hơn 3 tấn rác thải các loại, chủ yếu là túi ni lông, chai nhựa và rác sinh hoạt bị vứt xuống lòng kênh. Đây là hoạt động thường niên nhằm tuyên truyền ý thức bảo vệ môi trường nước cho cư dân xung quanh.",
                    Tag = "Tin tức",
                    TagClass = "tag-news",
                    IssuingUnit = "Quận Đoàn 1",
                    PublishedDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc),
                    IsActive = true,
                    DocumentNumber = "",
                    DocumentType = "Tin tức",
                    Signer = "",
                    SourceUrl = "https://tuoitre.vn",
                    EffectiveDate = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Cấu hình bảng DonationCategory
            modelBuilder.Entity<DonationCategory>(entity =>
            {
                entity.Property(dc => dc.TargetAmount).HasColumnType("decimal(18,2)");
                entity.Property(dc => dc.CurrentAmount).HasColumnType("decimal(18,2)");
            });

            // Cấu hình bảng Donation
            modelBuilder.Entity<Donation>(entity =>
            {
                entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(d => d.OrderId).IsUnique();

                entity.HasOne(d => d.DonationCategory)
                    .WithMany(dc => dc.Donations)
                    .HasForeignKey(d => d.DonationCategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SmartRoutingRule>(entity =>
            {
                entity.HasOne(r => r.TargetUnit)
                    .WithMany()
                    .HasForeignKey(r => r.TargetUnitId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Shift>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting { SettingKey = "MaintenanceMode", SettingValue = "False", Description = "Bật/Tắt chế độ bảo trì hệ thống" },
                new SystemSetting { SettingKey = "OrganizationName", SettingValue = "CivicConnect", Description = "Tên tổ chức vận hành chính thức" },
                new SystemSetting { SettingKey = "SystemLogoUrl", SettingValue = "", Description = "Đường dẫn URL ảnh logo hệ thống" }
            );

            // Seed các Quỹ Quyên Góp mẫu
            modelBuilder.Entity<DonationCategory>().HasData(
                new DonationCategory
                {
                    Id = 1,
                    Name = "Quỹ Trồng Xanh Đô Thị",
                    Description = "Quyên góp mua cây xanh, hoa trang trí trồng tại các tuyến ngõ hẻm, công viên công cộng trên địa bàn Phường Bến Nghé.",
                    TargetAmount = 50000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new DonationCategory
                {
                    Id = 2,
                    Name = "Quỹ Thắp Sáng Ngõ Hẻm",
                    Description = "Hỗ trợ lắp đặt hệ thống đèn đường LED thông minh, tiết kiệm điện tại các ngõ hẻm chưa có đủ ánh sáng.",
                    TargetAmount = 30000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new DonationCategory
                {
                    Id = 3,
                    Name = "Quỹ Nâng Cấp Sân Chơi Trẻ Em",
                    Description = "Mua sắm, lắp đặt và sửa chữa các thiết bị vui chơi ngoài trời tại điểm sinh hoạt cộng đồng của phường.",
                    TargetAmount = 100000000,
                    CurrentAmount = 0,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed dữ liệu mẫu cho Thủ tục hành chính (Phase 5)
            modelBuilder.Entity<AdministrativeProcedure>().HasData(
                new AdministrativeProcedure
                {
                    Id = 1,
                    Title = "Đăng ký khai sinh lưu động",
                    Code = "TTHC-01",
                    LegalBasis = "Luật Hộ tịch 2014, Nghị định 123/2015/NĐ-CP",
                    Category = "Hộ tịch",
                    Description = "Cán bộ tư pháp xuống tận nhà dân để làm thủ tục đăng ký khai sinh đối với các trường hợp đặc biệt khó khăn, khuyết tật.",
                    RequiredDocuments = "- Tờ khai đăng ký khai sinh\n- Giấy chứng sinh (hoặc văn bản làm chứng)\n- Thẻ CCCD của người yêu cầu",
                    ProcessingTime = "1 ngày làm việc",
                    Fee = "Miễn phí",
                    SubmissionPlace = "Nhà dân / Bộ phận một cửa UBND Phường",
                    TemplateUrl = "https://dichvucong.gov.vn",
                    IsActive = true
                },
                new AdministrativeProcedure
                {
                    Id = 2,
                    Title = "Xác nhận tình trạng hôn nhân",
                    Code = "TTHC-02",
                    LegalBasis = "Luật Hộ tịch 2014",
                    Category = "Hộ tịch",
                    Description = "Cấp giấy xác nhận tình trạng hôn nhân để làm thủ tục vay vốn, mua bán đất, hoặc đăng ký kết hôn.",
                    RequiredDocuments = "- Tờ khai yêu cầu xác nhận\n- Thẻ CCCD\n- Quyết định ly hôn (nếu đã từng ly hôn)",
                    ProcessingTime = "3 ngày làm việc",
                    Fee = "15.000 VNĐ",
                    SubmissionPlace = "Bộ phận một cửa UBND Phường Bến Nghé",
                    TemplateUrl = "https://dichvucong.gov.vn",
                    IsActive = true
                },
                new AdministrativeProcedure
                {
                    Id = 3,
                    Title = "Cấp bản sao trích lục hộ tịch",
                    Code = "TTHC-03",
                    LegalBasis = "Luật Hộ tịch 2014",
                    Category = "Hộ tịch",
                    Description = "Cấp bản sao trích lục từ sổ gốc hộ tịch (Khai sinh, Kết hôn, Khai tử).",
                    RequiredDocuments = "- Tờ khai yêu cầu cấp bản sao\n- Thẻ CCCD",
                    ProcessingTime = "Trả kết quả ngay trong ngày",
                    Fee = "8.000 VNĐ/bản",
                    SubmissionPlace = "Bộ phận một cửa UBND Phường Bến Nghé",
                    TemplateUrl = "https://dichvucong.gov.vn",
                    IsActive = true
                }
            );

            // Seed dữ liệu mẫu cho Danh bạ cơ quan (Phase 5)
            modelBuilder.Entity<AgencyDirectory>().HasData(
                new AgencyDirectory
                {
                    Id = 1,
                    Name = "Công an Phường Bến Nghé",
                    Type = "Công an",
                    Phone = "02838297335",
                    Email = "congan.bennghe@tphcm.gov.vn",
                    Address = "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TPHCM",
                    WorkingHours = "Trực 24/7",
                    Latitude = 10.7811,
                    Longitude = 106.7051,
                    Rating = 4.8f,
                    ReceptionSchedule = "Tiếp công dân hằng ngày vào giờ hành chính (Trưởng công an phường tiếp vào sáng Thứ 5).",
                    IsEmergency = true,
                    OrderIndex = 1
                },
                new AgencyDirectory
                {
                    Id = 2,
                    Name = "Trạm Y tế Phường Bến Nghé",
                    Type = "Y tế",
                    Phone = "02838222956",
                    Email = "tramyte.bennghe@tphcm.gov.vn",
                    Address = "Số 1 Lý Tự Trọng, Bến Nghé, Quận 1, TPHCM",
                    WorkingHours = "07:30 - 16:30 (Thứ 2 - Thứ 6)",
                    Latitude = 10.7788,
                    Longitude = 106.7032,
                    Rating = 4.5f,
                    ReceptionSchedule = "Tiêm chủng định kỳ vào sáng Thứ 4 và Thứ 6 hằng tuần.",
                    IsEmergency = false,
                    OrderIndex = 2
                },
                new AgencyDirectory
                {
                    Id = 3,
                    Name = "Công ty Cấp nước Bến Thành",
                    Type = "Hạ tầng",
                    Phone = "19001224",
                    Email = "cskh@capnuocbenthanh.com",
                    Address = "194 Pasteur, Phường Võ Thị Sáu, Quận 3, TPHCM",
                    WorkingHours = "08:00 - 17:00 (Thứ 2 - Thứ 6)",
                    Latitude = 10.7830,
                    Longitude = 106.6940,
                    Rating = 4.0f,
                    ReceptionSchedule = "Tiếp nhận hồ sơ lắp đặt mới tại quầy vào giờ hành chính.",
                    IsEmergency = false,
                    OrderIndex = 3
                },
                new AgencyDirectory
                {
                    Id = 4,
                    Name = "Lịch tiếp dân Chủ tịch UBND Phường Bến Nghé",
                    Type = "Hành chính",
                    Phone = "02838290290",
                    Email = "bennghe.q1@tphcm.gov.vn",
                    Address = "29 Nguyễn Trung Ngạn, Bến Nghé, Quận 1, TPHCM",
                    WorkingHours = "Sáng Thứ 3 & Thứ 5 hằng tuần",
                    Latitude = 10.7811,
                    Longitude = 106.7051,
                    Rating = 5.0f,
                    ReceptionSchedule = "Đồng chí Chủ tịch UBND Phường tiếp công dân định kỳ để giải quyết khiếu nại, tố cáo và các vấn đề dân sinh phức tạp.",
                    IsEmergency = false,
                    OrderIndex = 0
                }
            );
        }
    }
}
