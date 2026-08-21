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
            Name = "Atlas Distribution",
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

        await SeedCustomersAsync(context, tenant.Id);
        await SeedInvoicesAsync(context, tenant.Id);

        await context.SaveChangesAsync();
    }

    private static async Task SeedCustomersAsync(ApplicationDbContext context, Guid tenantId)
    {
        var customers = new[]
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST-001",
                Name = "ABC Distribution SPA",
                LegalName = "ABC Distribution Société par Actions",
                TradeRegister = "16B123456",
                TaxId = "123456789012345",
                Industry = "Distribution",
                Address = "Rue Didouche Mourad, Alger",
                City = "Alger",
                Country = "Algeria",
                PhoneNumber = "+213 21 00 00 01",
                Email = "contact@abcdistribution.dz",
                PaymentTermsDays = 30,
                CreditLimit = 20000000,
                IsActive = true
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Code = "CUST-002",
                Name = "BTP Nord",
                LegalName = "BTP Nord SARL",
                TradeRegister = "16B654321",
                TaxId = "987654321098765",
                Industry = "BTP",
                Address = "Zone industrielle, Oran",
                City = "Oran",
                Country = "Algeria",
                PhoneNumber = "+213 41 00 00 02",
                Email = "finance@btpnord.dz",
                PaymentTermsDays = 45,
                CreditLimit = 50000000,
                IsActive = true
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
    }

    private static async Task SeedInvoicesAsync(ApplicationDbContext context, Guid tenantId)
    {
        var customers = await context.Customers.Where(c => c.TenantId == tenantId).ToListAsync();
        var customer1 = customers.First(c => c.Code == "CUST-001");
        var customer2 = customers.First(c => c.Code == "CUST-002");

        var invoices = new[]
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customer1.Id,
                InvoiceNumber = "INV-2024-001",
                InvoiceDate = DateTime.UtcNow.AddDays(-60),
                DueDate = DateTime.UtcNow.AddDays(-30),
                Amount = 1500000,
                PaidAmount = 0,
                Currency = "DZD",
                Status = InvoiceStatus.Overdue,
                Description = "Fourniture équipement électrique"
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customer1.Id,
                InvoiceNumber = "INV-2024-002",
                InvoiceDate = DateTime.UtcNow.AddDays(-15),
                DueDate = DateTime.UtcNow.AddDays(15),
                Amount = 2500000,
                PaidAmount = 0,
                Currency = "DZD",
                Status = InvoiceStatus.Open,
                Description = "Fourniture câbles industriels"
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerId = customer2.Id,
                InvoiceNumber = "INV-2024-003",
                InvoiceDate = DateTime.UtcNow.AddDays(-90),
                DueDate = DateTime.UtcNow.AddDays(-45),
                Amount = 5000000,
                PaidAmount = 1500000,
                Currency = "DZD",
                Status = InvoiceStatus.PartiallyPaid,
                Description = "Prestations génie civil"
            }
        };

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();
    }
}
