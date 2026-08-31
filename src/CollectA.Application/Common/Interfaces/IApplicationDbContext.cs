using CollectA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerContact> CustomerContacts { get; }
    DbSet<CustomerAssignment> CustomerAssignments { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Cheque> Cheques { get; }
    DbSet<CollectionAction> CollectionActions { get; }
    DbSet<CollectionTask> CollectionTasks { get; }
    DbSet<PromiseToPay> PromiseToPays { get; }
    DbSet<Dispute> Disputes { get; }
    DbSet<RiskScore> RiskScores { get; }
    DbSet<CreditLimit> CreditLimits { get; }
    DbSet<ReminderTemplate> ReminderTemplates { get; }
    DbSet<ReminderRule> ReminderRules { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Integration> Integrations { get; }
    DbSet<IntegrationLog> IntegrationLogs { get; }
    DbSet<Currency> Currencies { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<Document> Documents { get; }
    DbSet<Note> Notes { get; }
    DbSet<Tag> Tags { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
