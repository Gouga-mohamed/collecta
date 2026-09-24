using CollectA.Domain.Entities;
using CollectA.Domain.Enums;
using CollectA.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Infrastructure.Persistence.Seed;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        if (await context.Tenants.AnyAsync()) return;

        var tenant = new Tenant
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Atlas Distribution SPA",
            Subdomain = "atlas-distribution",
            Currency = "DZD",
            Language = "fr",
            TimeZone = "Africa/Algiers",
            SubscriptionPlan = "Pro"
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var ownerRole = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = "Owner",
            NormalizedName = "OWNER",
            TenantId = tenant.Id,
            IsBuiltIn = true
        };
        context.Roles.Add(ownerRole);
        await context.SaveChangesAsync();

        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "admin@atlas-distribution.dz",
            Email = "admin@atlas-distribution.dz",
            NormalizedEmail = "ADMIN@ATLAS-DISTRIBUTION.DZ",
            NormalizedUserName = "ADMIN@ATLAS-DISTRIBUTION.DZ",
            FirstName = "Mohamed",
            LastName = "Benali",
            TenantId = tenant.Id,
            EmailConfirmed = true,
            LockoutEnabled = false
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Owner");

        var today = DateTime.UtcNow.Date;

        var customers = BuildCustomers(tenant.Id);
        context.Customers.AddRange(customers);
        context.CustomerContacts.AddRange(BuildContacts(tenant.Id, customers));

        var invoices = BuildInvoices(tenant.Id, customers, today);
        context.Invoices.AddRange(invoices);

        var (payments, cheques) = BuildPayments(tenant.Id, invoices, today);
        context.Cheques.AddRange(cheques);
        context.Payments.AddRange(payments);

        await context.SaveChangesAsync();
    }

    private sealed record CustomerSeed(
        string Code, string Name, string LegalName, string Industry, string City, string Address,
        string PhoneNumber, string Email, string TaxId, string TradeRegister,
        int PaymentTermsDays, decimal CreditLimit);

    private static List<Customer> BuildCustomers(Guid tenantId)
    {
        var seeds = new[]
        {
            new CustomerSeed("CUST-001", "Pharmacie El Yasmine", "Pharmacie El Yasmine EURL", "Pharmacie / Parapharmacie",
                "Alger", "12, rue Hassiba Ben Bouali, Alger Centre", "+213 550 12 34 56", "contact@pharmacie-elyasmine.dz",
                "099816012345678", "16B0123456", 30, 8_000_000m),
            new CustomerSeed("CUST-002", "AgroDis Sud", "EURL AgroDis Sud", "Distribution agroalimentaire",
                "Blida", "Zone industrielle Ouled Yaïch, Blida", "+213 661 45 67 89", "commandes@agrodis-sud.dz",
                "099916012345679", "16B0654321", 45, 25_000_000m),
            new CustomerSeed("CUST-003", "Gros Œuvre Bâtiments Est", "Gros Œuvre Bâtiments Est SARL", "BTP / Génie civil",
                "Constantine", "Cité des entrepreneurs, Constantine", "+213 792 34 56 78", "achats@gobe-constantine.dz",
                "099716012345670", "16B0987654", 60, 60_000_000m),
            new CustomerSeed("CUST-004", "Benali Frères Électroménager", "Ets Benali Frères Électroménager SARL", "Électroménager",
                "Sétif", "Avenue du 8 Mai 1945, Sétif", "+213 550 87 65 43", "frs.benali@benalielectro.dz",
                "099616012345671", "16B0456789", 30, 15_000_000m),
            new CustomerSeed("CUST-005", "Nord Lait Distribution", "SPA Nord Lait Distribution", "Produits laitiers",
                "Oran", "Route de Belgaïd, Oran", "+213 662 11 22 33", "logistique@nordlait.dz",
                "099516012345672", "16B0789123", 45, 30_000_000m),
            new CustomerSeed("CUST-006", "Pharmacie du Centre", "EURL PharmaCen", "Pharmacie",
                "Annaba", "Boulevard du 1er Novembre, Annaba", "+213 771 99 88 77", "pharmacie.centre@pharmcen.dz",
                "099416012345673", "16B0345678", 30, 6_000_000m),
            new CustomerSeed("CUST-007", "BTP Horizon Travaux", "BTP Horizon Travaux SPA", "BTP / Promotion immobilière",
                "Alger", "Lotissement des Pins, Draria, Alger", "+213 550 44 55 66", "comptabilite@horizon-travaux.dz",
                "099316012345674", "16B0876543", 90, 80_000_000m),
            new CustomerSeed("CUST-008", "Electro Atlas Magasin", "EURL Electro Atlas", "Électroménager / High-tech",
                "Alger", "Rue des Frères Bouadou, Bab El Oued, Alger", "+213 663 77 88 99", "gerance@electro-atlas.dz",
                "099216012345675", "16B0567891", 60, 20_000_000m),
            new CustomerSeed("CUST-009", "Humasud Distribution", "EURL Humasud Distribution", "Distribution agroalimentaire",
                "Blida", "Rue Émile Benoist, Blida", "+213 799 66 55 44", "contact@humasud-dz.com",
                "099116012345676", "16B0234567", 30, 12_000_000m),
            new CustomerSeed("CUST-010", "Grossiste El Anka", "EURL Grossiste El Anka", "Grossiste alimentaire",
                "Oran", "Zone industrienne Es-Sénia, Oran", "+213 551 33 22 11", "elanka.grossiste@elanka.dz",
                "099016012345677", "16B0678912", 45, 40_000_000m),
            new CustomerSeed("CUST-011", "Pharmacie Essalem", "EURL Pharmacie Essalem", "Pharmacie",
                "Constantine", "Rue Didouche Mourad, Constantine", "+213 670 55 66 77", "essalem.pharmacie@essalem.dz",
                "099816012345680", "16B0123987", 60, 5_000_000m)
        };

        return seeds.Select(s => new Customer
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = s.Code,
            Name = s.Name,
            LegalName = s.LegalName,
            Industry = s.Industry,
            Address = s.Address,
            City = s.City,
            Country = "Algeria",
            PhoneNumber = s.PhoneNumber,
            Email = s.Email,
            TaxId = s.TaxId,
            TradeRegister = s.TradeRegister,
            PaymentTermsDays = s.PaymentTermsDays,
            CreditLimit = s.CreditLimit,
            IsActive = true
        }).ToList();
    }

    private static List<CustomerContact> BuildContacts(Guid tenantId, List<Customer> customers)
    {
        var customerId = customers.First(c => c.Code == "CUST-001").Id;

        return new List<CustomerContact>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerId,
                FirstName = "Yacine",
                LastName = "Merbah",
                JobTitle = "Gérant",
                Email = "y.merbah@pharmacie-elyasmine.dz",
                PhoneNumber = "+213 550 12 34 56",
                IsPrimary = true,
                IsBillingContact = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerId,
                FirstName = "Amel",
                LastName = "Boudjema",
                JobTitle = "Responsable comptabilité",
                Email = "a.boudjema@pharmacie-elyasmine.dz",
                PhoneNumber = "+213 661 23 45 67",
                IsPrimary = false,
                IsBillingContact = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerId,
                FirstName = "Sofiane",
                LastName = "Kaci",
                JobTitle = "Assistant administratif",
                Email = "s.kaci@pharmacie-elyasmine.dz",
                PhoneNumber = "+213 792 34 56 78",
                IsPrimary = false,
                IsBillingContact = false
            }
        };
    }

    private sealed record InvoiceSeed(
        string CustomerCode, int DaysAgoInvoiced, int TermsDays, decimal Amount, decimal PaidAmount,
        string Description, bool Disputed = false, bool Draft = false);

    private static List<Invoice> BuildInvoices(Guid tenantId, List<Customer> customers, DateTime today)
    {
        var seeds = new[]
        {
            // Buckets courants (échéance dans le futur)
            new InvoiceSeed("CUST-001", 10, 30, 890_000m, 0m, "Fourniture médicaments génériques"),
            new InvoiceSeed("CUST-003", 20, 60, 7_800_000m, 0m, "Fourniture ciment et agrégats"),
            new InvoiceSeed("CUST-004", 30, 30, 1_680_000m, 0m, "Livraison réfrigérateurs et congélateurs"),
            new InvoiceSeed("CUST-005", 12, 45, 5_400_000m, 0m, "Livraison lait UHT et dérivés"),
            new InvoiceSeed("CUST-006", 5, 30, 1_320_000m, 0m, "Fourniture produits dermo-cosmétiques"),
            new InvoiceSeed("CUST-007", 15, 90, 12_000_000m, 0m, "Fourniture matériaux pour chantier Hussein Dey"),

            // Litige : facture disputée non échue
            new InvoiceSeed("CUST-003", 25, 60, 4_200_000m, 0m, "Prestation terrassement – litige sur quantités livrées", Disputed: true),

            // 1-30 jours de retard
            new InvoiceSeed("CUST-001", 45, 30, 1_250_000m, 0m, "Fourniture produits parapharmaceutiques"),
            new InvoiceSeed("CUST-002", 60, 45, 4_800_000m, 2_000_000m, "Livraison conserves alimentaires"),
            new InvoiceSeed("CUST-008", 90, 60, 4_100_000m, 4_100_000m, "Livraison téléviseurs et électroménager"),
            new InvoiceSeed("CUST-009", 50, 30, 1_700_000m, 1_700_000m, "Livraison huiles et semoule"),
            new InvoiceSeed("CUST-010", 65, 45, 9_200_000m, 9_200_000m, "Livraison denrées alimentaires grande distribution"),
            new InvoiceSeed("CUST-006", 40, 30, 1_150_000m, 1_150_000m, "Fourniture médicaments cardiologie"),

            // 31-60 jours
            new InvoiceSeed("CUST-001", 70, 30, 1_540_000m, 1_540_000m, "Fourniture dispositifs médicaux"),
            new InvoiceSeed("CUST-003", 105, 60, 8_500_000m, 0m, "Fourniture fer à béton et acier"),
            new InvoiceSeed("CUST-004", 75, 30, 2_350_000m, 0m, "Livraison machines à laver"),
            new InvoiceSeed("CUST-005", 90, 45, 7_100_000m, 7_100_000m, "Livraison yaourts et desserts lactés"),
            new InvoiceSeed("CUST-006", 85, 30, 980_000m, 0m, "Fourniture compléments alimentaires"),

            // 61-90 jours
            new InvoiceSeed("CUST-002", 120, 45, 6_200_000m, 0m, "Livraison huiles alimentaires"),
            new InvoiceSeed("CUST-003", 150, 60, 11_200_000m, 3_000_000m, "Fourniture bois et matériaux de coffrage"),
            new InvoiceSeed("CUST-004", 100, 30, 2_900_000m, 2_900_000m, "Livraison climatiseurs"),
            new InvoiceSeed("CUST-005", 135, 45, 6_500_000m, 0m, "Livraison fromages et beurre"),
            new InvoiceSeed("CUST-007", 180, 90, 10_500_000m, 4_000_000m, "Fourniture pour programme immobilier El Biar"),

            // 91-120 jours
            new InvoiceSeed("CUST-008", 165, 60, 3_300_000m, 0m, "Livraison smartphones et accessoires"),
            new InvoiceSeed("CUST-009", 135, 30, 1_950_000m, 0m, "Livraison légumes secs et épices"),
            new InvoiceSeed("CUST-011", 160, 60, 1_480_000m, 0m, "Fourniture médicaments pédiatrie"),

            // 121-180 jours
            new InvoiceSeed("CUST-008", 220, 60, 2_750_000m, 1_000_000m, "Livraison matériel électroménager"),
            new InvoiceSeed("CUST-009", 180, 30, 2_400_000m, 0m, "Livraison conserves et condiments"),
            new InvoiceSeed("CUST-011", 200, 60, 1_120_000m, 0m, "Fourniture produits ORL"),

            // > 180 jours
            new InvoiceSeed("CUST-003", 255, 60, 9_600_000m, 0m, "Fourniture granulats et béton prêt à l'emploi"),
            new InvoiceSeed("CUST-010", 240, 45, 8_900_000m, 2_500_000m, "Livraison sucre et farine"),
            new InvoiceSeed("CUST-010", 255, 45, 7_600_000m, 0m, "Livraison riz et légumineuses"),

            // Brouillons
            new InvoiceSeed("CUST-001", 2, 30, 640_000m, 0m, "Commande en préparation – parapharmacie", Draft: true),
            new InvoiceSeed("CUST-005", 3, 45, 3_800_000m, 0m, "Bon de commande laiterie – à confirmer", Draft: true)
        };

        var customerByCode = customers.ToDictionary(c => c.Code);

        return seeds.Select((s, index) =>
        {
            var invoiceDate = today.AddDays(-s.DaysAgoInvoiced);
            var dueDate = invoiceDate.AddDays(s.TermsDays);

            return new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customerByCode[s.CustomerCode].Id,
                InvoiceNumber = $"INV-2025-{index + 1:000}",
                InvoiceDate = invoiceDate,
                DueDate = dueDate,
                Amount = s.Amount,
                PaidAmount = s.PaidAmount,
                Currency = "DZD",
                Status = ResolveStatus(s, dueDate, today),
                Description = s.Description,
                PaymentTermsDays = s.TermsDays,
                IsDisputed = s.Disputed,
                DisputedAt = s.Disputed ? invoiceDate.AddDays(10) : null
            };
        }).ToList();
    }

    private static InvoiceStatus ResolveStatus(InvoiceSeed seed, DateTime dueDate, DateTime today)
    {
        if (seed.Draft) return InvoiceStatus.Draft;
        if (seed.PaidAmount >= seed.Amount) return InvoiceStatus.Paid;
        if (seed.PaidAmount > 0) return InvoiceStatus.PartiallyPaid;
        if (seed.Disputed) return InvoiceStatus.Disputed;
        if (dueDate < today) return InvoiceStatus.Overdue;
        return InvoiceStatus.Open;
    }

    private sealed record ChequeSeed(
        string ChequeNumber, string BankName, string Drawer, ChequeStatus Status, int DaysUntilDue);

    private sealed record PaymentSeed(
        string InvoiceNumber, decimal Amount, PaymentMethod Method, string? BankName,
        string? Reference, int DaysAfterInvoiceDate, ChequeSeed? Cheque = null);

    private static (List<Payment> Payments, List<Cheque> Cheques) BuildPayments(
        Guid tenantId, List<Invoice> invoices, DateTime today)
    {
        var seeds = new[]
        {
            new PaymentSeed("INV-2025-009", 2_000_000m, PaymentMethod.Cheque, null, null, 30,
                new ChequeSeed("000112233", "Al Baraka Bank", "AgroDis Sud EURL", ChequeStatus.Rejected, 60)),
            new PaymentSeed("INV-2025-010", 4_100_000m, PaymentMethod.BankTransfer, "AGB", "VIR-2025-01345", 28),
            new PaymentSeed("INV-2025-011", 1_700_000m, PaymentMethod.Cheque, null, null, 35,
                new ChequeSeed("000778899", "BDL", "Humasud Distribution EURL", ChequeStatus.Received, 60)),
            new PaymentSeed("INV-2025-012", 5_200_000m, PaymentMethod.BankTransfer, "Al Baraka Bank", "VIR-2025-01556", 40),
            new PaymentSeed("INV-2025-012", 4_000_000m, PaymentMethod.Cheque, null, null, 55,
                new ChequeSeed("000334455", "AGB", "Grossiste El Anka EURL", ChequeStatus.Cleared, 90)),
            new PaymentSeed("INV-2025-013", 1_150_000m, PaymentMethod.Cheque, null, null, 20,
                new ChequeSeed("000556677", "BEA", "Salah Meziane", ChequeStatus.Cleared, 45)),
            new PaymentSeed("INV-2025-014", 1_540_000m, PaymentMethod.BankTransfer, "BDL", "VIR-2025-00412", 25),
            new PaymentSeed("INV-2025-017", 7_100_000m, PaymentMethod.BankTransfer, "BNA", "VIR-2025-00890", 40),
            new PaymentSeed("INV-2025-020", 3_000_000m, PaymentMethod.Cheque, null, null, 80,
                new ChequeSeed("000124578", "BNA", "Gros Œuvre Bâtiments Est SARL", ChequeStatus.Deposited, 90)),
            new PaymentSeed("INV-2025-021", 2_900_000m, PaymentMethod.BankTransfer, "CPA", "VIR-2025-00977", 25),
            new PaymentSeed("INV-2025-023", 2_500_000m, PaymentMethod.BankTransfer, "BNA", "VIR-2025-00231", 60),
            new PaymentSeed("INV-2025-023", 1_500_000m, PaymentMethod.Cash, null, "ESPECES-118", 105),
            new PaymentSeed("INV-2025-027", 1_000_000m, PaymentMethod.Cheque, null, null, 150,
                new ChequeSeed("000987654", "CPA", "Karim Haddad", ChequeStatus.Returned, 30)),
            new PaymentSeed("INV-2025-031", 1_500_000m, PaymentMethod.BankTransfer, "CPA", "VIR-2025-00319", 105),
            new PaymentSeed("INV-2025-031", 1_000_000m, PaymentMethod.Cash, null, "ESPECES-207", 120)
        };

        var invoiceByNumber = invoices.ToDictionary(i => i.InvoiceNumber);
        var payments = new List<Payment>();
        var cheques = new List<Cheque>();

        foreach (var s in seeds)
        {
            var invoice = invoiceByNumber[s.InvoiceNumber];
            var paymentDate = invoice.InvoiceDate.AddDays(s.DaysAfterInvoiceDate);
            if (paymentDate > today) paymentDate = today;

            Cheque? cheque = null;
            if (s.Cheque != null)
            {
                cheque = new Cheque
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    CustomerId = invoice.CustomerId,
                    ChequeNumber = s.Cheque.ChequeNumber,
                    BankName = s.Cheque.BankName,
                    Drawer = s.Cheque.Drawer,
                    Amount = s.Amount,
                    Currency = "DZD",
                    IssueDate = paymentDate,
                    DueDate = paymentDate.AddDays(s.Cheque.DaysUntilDue),
                    DepositDate = s.Cheque.Status is ChequeStatus.Deposited or ChequeStatus.Cleared
                        ? paymentDate.AddDays(3)
                        : null,
                    Status = s.Cheque.Status,
                    Notes = s.Cheque.Status == ChequeStatus.Rejected ? "Chèque rejeté – provision insuffisante" : null
                };
                cheques.Add(cheque);
            }

            payments.Add(new Payment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = invoice.CustomerId,
                InvoiceId = invoice.Id,
                Amount = s.Amount,
                Currency = "DZD",
                PaymentDate = paymentDate,
                Method = s.Method,
                Reference = s.Reference ?? s.Cheque?.ChequeNumber,
                BankName = s.BankName ?? s.Cheque?.BankName,
                Notes = s.Method == PaymentMethod.Cash ? "Encaissement espèces sur site" : null,
                ChequeId = cheque?.Id,
                Cheque = cheque
            });
        }

        return (payments, cheques);
    }
}
