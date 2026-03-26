using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProposalManagement.Domain.Entities;
using ProposalManagement.Domain.Enums;
using ProposalManagement.Infrastructure.Persistence;

namespace ProposalManagement.Infrastructure.Services;

public class DbInitializationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbInitializationService> _logger;

    public DbInitializationService(AppDbContext context, ILogger<DbInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Database migration completed");

        await SeedCorporationSettingsAsync(cancellationToken);
        await SeedDefaultLotusUserAsync(cancellationToken);
        await SeedDepartmentsAsync(cancellationToken);
        await SeedDesignationsAsync(cancellationToken);
        await SeedFundTypesAsync(cancellationToken);
        await SeedAccountHeadsAsync(cancellationToken);
        await SeedWardsAsync(cancellationToken);
        await SeedProcurementMethodsAsync(cancellationToken);
        await SeedTenderPublicationPeriodsAsync(cancellationToken);
        await SeedTestUsersAsync(cancellationToken);

        _logger.LogInformation("Seed data completed");
    }

    private async Task SeedCorporationSettingsAsync(CancellationToken cancellationToken)
    {
        if (await _context.CorporationSettings.AnyAsync(cancellationToken))
            return;

        _context.CorporationSettings.Add(new CorporationSettings
        {
            Id = 1,
            CorporationName_En = "Dhule Municipal Corporation",
            CorporationName_Alt = "धुळे महानगरपालिका",
            PrimaryLanguage = "en",
            AlternateLanguage = "mr",
            AlternateLanguageLabel = "मराठी",
            DefaultDisplayLanguage = "en",
            AutoTranslateEnabled = true,
            SmsGatewayProvider = "Development",
            SmsGatewayApiKey = "",
            OtpExpiryMinutes = 5,
            OtpMaxAttempts = 3,
            LotusSessionTimeoutMinutes = 15,
            UpdatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDefaultLotusUserAsync(CancellationToken cancellationToken)
    {
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Role == UserRole.Lotus, cancellationToken))
            return;

        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName_En = "System Administrator",
            FullName_Alt = "सिस्टम प्रशासक",
            MobileNumber = "9999999999",
            Email = "admin@dmc.gov.in",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = UserRole.Lotus,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDepartmentsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Departments.AnyAsync(cancellationToken))
            return;

        var departments = new List<Department>
        {
            new() { Id = Guid.NewGuid(), Name_En = "City Engineering", Name_Alt = "शहर अभियांत्रिकी", Code = "CE", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Accounts", Name_Alt = "लेखा", Code = "ACC", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Water Supply", Name_Alt = "पाणी पुरवठा", Code = "WS", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Health", Name_Alt = "आरोग्य", Code = "HLT", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Town Planning", Name_Alt = "नगर रचना", Code = "TP", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.Departments.AddRange(departments);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedDesignationsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Designations.AnyAsync(cancellationToken))
            return;

        var designations = new List<Designation>
        {
            new() { Id = Guid.NewGuid(), Name_En = "City Engineer", Name_Alt = "शहर अभियंता", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Chief Accountant", Name_Alt = "मुख्य लेखापाल", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Deputy Commissioner", Name_Alt = "उप आयुक्त", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Commissioner", Name_Alt = "आयुक्त", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Junior Engineer", Name_Alt = "कनिष्ठ अभियंता", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Executive Engineer", Name_Alt = "कार्यकारी अभियंता", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.Designations.AddRange(designations);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedFundTypesAsync(CancellationToken cancellationToken)
    {
        if (await _context.FundTypes.AnyAsync(cancellationToken))
            return;

        var items = new List<FundType>
        {
            new() { Id = Guid.NewGuid(), Name_En = "General Fund", Name_Alt = "सामान्य निधी", IsMnp = true, IsState = false, IsCentral = false, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Water Supply Fund", Name_Alt = "पाणी पुरवठा निधी", IsMnp = true, IsState = true, IsCentral = false, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "SFC Grant", Name_Alt = "SFC अनुदान", IsMnp = false, IsState = true, IsCentral = false, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "15th Finance Commission", Name_Alt = "१५वा वित्त आयोग", IsMnp = false, IsState = false, IsCentral = true, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Water Collection", Name_Alt = "जलसंकलन", IsMnp = true, IsState = false, IsCentral = false, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Property Tax", Name_Alt = "मालमत्ता कर", IsMnp = true, IsState = false, IsCentral = false, IsDpdc = false, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "DPDC Fund", Name_Alt = "डीपीडीसी निधी", IsMnp = true, IsState = false, IsCentral = false, IsDpdc = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.FundTypes.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAccountHeadsAsync(CancellationToken cancellationToken)
    {
        if (await _context.AccountHeads.AnyAsync(cancellationToken))
            return;

        var items = new List<AccountHead>
        {
            new() { Id = Guid.NewGuid(), Code = "4210", Name_En = "Capital Expenditure on Urban Development", Name_Alt = "नगर विकास भांडवली खर्च", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "2217", Name_En = "Urban Development - Revenue", Name_Alt = "नगर विकास - महसूल", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "2215", Name_En = "Water Supply & Sanitation", Name_Alt = "पाणी पुरवठा व स्वच्छता", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Code = "3054", Name_En = "Roads and Bridges", Name_Alt = "रस्ते व पूल", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.AccountHeads.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedWardsAsync(CancellationToken cancellationToken)
    {
        if (await _context.Wards.AnyAsync(cancellationToken))
            return;

        var items = new List<Ward>();
        var wardNames = new[] {
            ("Ward 1 - Deopur", "प्रभाग १ - देवपूर"),
            ("Ward 2 - Kholi Galli", "प्रभाग २ - खोली गल्ली"),
            ("Ward 3 - Sakri Naka", "प्रभाग ३ - साक्री नाका"),
            ("Ward 4 - Lane 5", "प्रभाग ४ - लेन ५"),
            ("Ward 5 - Datta Mandir", "प्रभाग ५ - दत्त मंदिर"),
        };

        for (int i = 0; i < wardNames.Length; i++)
        {
            items.Add(new Ward
            {
                Id = Guid.NewGuid(),
                Number = i + 1,
                Name_En = wardNames[i].Item1,
                Name_Alt = wardNames[i].Item2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Wards.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedProcurementMethodsAsync(CancellationToken cancellationToken)
    {
        if (await _context.ProcurementMethods.AnyAsync(cancellationToken))
            return;

        var items = new List<ProcurementMethod>
        {
            new() { Id = Guid.NewGuid(), Name_En = "Open Tender", Name_Alt = "खुली निविदा", Description_En = "Public competitive bidding", Description_Alt = "सार्वजनिक स्पर्धात्मक निविदा", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Limited Tender", Name_Alt = "मर्यादित निविदा", Description_En = "Invitation to limited bidders", Description_Alt = "मर्यादित निविदाकारांना आमंत्रण", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Quotation", Name_Alt = "दरपत्रक", Description_En = "Direct quotation from vendors", Description_Alt = "विक्रेत्यांकडून थेट दरपत्रक", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name_En = "Direct Purchase", Name_Alt = "थेट खरेदी", Description_En = "Direct procurement without bidding", Description_Alt = "निविदाशिवाय थेट खरेदी", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.ProcurementMethods.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTenderPublicationPeriodsAsync(CancellationToken cancellationToken)
    {
        if (await _context.TenderPublicationPeriods.AnyAsync(cancellationToken))
            return;

        var items = new List<TenderPublicationPeriod>
        {
            new() { Id = Guid.NewGuid(), MinAmount = 0, MaxAmount = 500000, DurationDays = 7, Description_En = "Up to 5 Lakh - 7 days", Description_Alt = "५ लाखापर्यंत - ७ दिवस", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MinAmount = 500001, MaxAmount = 2500000, DurationDays = 15, Description_En = "5 to 25 Lakh - 15 days", Description_Alt = "५ ते २५ लाख - १५ दिवस", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), MinAmount = 2500001, MaxAmount = 100000000, DurationDays = 30, Description_En = "Above 25 Lakh - 30 days", Description_Alt = "२५ लाखावरील - ३० दिवस", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.TenderPublicationPeriods.AddRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTestUsersAsync(CancellationToken cancellationToken)
    {
        // Only seed test users if no non-Lotus users exist
        if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Role != UserRole.Lotus, cancellationToken))
            return;

        var ceDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "CE", cancellationToken);
        var accDept = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "ACC", cancellationToken);

        var ceDesig = await _context.Designations.FirstOrDefaultAsync(d => d.Name_En == "City Engineer", cancellationToken);
        var caDesig = await _context.Designations.FirstOrDefaultAsync(d => d.Name_En == "Chief Accountant", cancellationToken);
        var dcDesig = await _context.Designations.FirstOrDefaultAsync(d => d.Name_En == "Deputy Commissioner", cancellationToken);
        var commDesig = await _context.Designations.FirstOrDefaultAsync(d => d.Name_En == "Commissioner", cancellationToken);
        var jeDesig = await _context.Designations.FirstOrDefaultAsync(d => d.Name_En == "Junior Engineer", cancellationToken);

        var testUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), FullName_En = "Rajesh Patil", FullName_Alt = "राजेश पाटील", MobileNumber = "9876543210", Role = UserRole.Submitter, DepartmentId = ceDept?.Id, DesignationId = jeDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FullName_En = "Sunil Jadhav", FullName_Alt = "सुनील जाधव", MobileNumber = "9876543211", Role = UserRole.CityEngineer, DepartmentId = ceDept?.Id, DesignationId = ceDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FullName_En = "Priya Deshmukh", FullName_Alt = "प्रिया देशमुख", MobileNumber = "9876543212", Role = UserRole.ChiefAccountant, DepartmentId = accDept?.Id, DesignationId = caDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FullName_En = "Amit Kulkarni", FullName_Alt = "अमित कुलकर्णी", MobileNumber = "9876543213", Role = UserRole.DeputyCommissioner, DepartmentId = ceDept?.Id, DesignationId = dcDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FullName_En = "Dr. Mahesh Sharma", FullName_Alt = "डॉ. महेश शर्मा", MobileNumber = "9876543214", Role = UserRole.Commissioner, DepartmentId = ceDept?.Id, DesignationId = commDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), FullName_En = "Sandeep Gaikwad", FullName_Alt = "संदीप गायकवाड", MobileNumber = "9876543215", Role = UserRole.Auditor, DepartmentId = accDept?.Id, DesignationId = caDesig?.Id, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };

        _context.Users.AddRange(testUsers);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} test users for all roles", testUsers.Count);
    }
}
