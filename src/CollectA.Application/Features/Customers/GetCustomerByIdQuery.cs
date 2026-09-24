using CollectA.Application.Features.Invoices;
using CollectA.Application.Features.Payments;
using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class GetCustomerByIdQuery : IRequest<CustomerDetailDto?>
{
    public Guid Id { get; set; }
}

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerDetailDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .ForTenant(_tenantContext)
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.CollectionActions)
            .Include(c => c.Promises)
            .Include(c => c.Disputes)
            .Include(c => c.Contacts)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) return null;

        var dto = GetCustomersQueryHandler.MapToDto(customer);
        var detail = new CustomerDetailDto
        {
            Id = dto.Id,
            Code = dto.Code,
            Name = dto.Name,
            LegalName = dto.LegalName,
            TaxId = dto.TaxId,
            Industry = dto.Industry,
            City = dto.City,
            Country = dto.Country,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            PaymentTermsDays = dto.PaymentTermsDays,
            CreditLimit = dto.CreditLimit,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            TotalInvoiced = dto.TotalInvoiced,
            TotalPaid = dto.TotalPaid,
            TotalDue = dto.TotalDue,
            TotalOverdue = dto.TotalOverdue,
            RiskLevel = dto.RiskLevel,
            TradeRegister = customer.TradeRegister,
            Address = customer.Address,
            Website = customer.Website,
            Notes = customer.Notes,
            Contacts = customer.Contacts.Select(x => new CustomerContactDto
            {
                Id = x.Id,
                Name = $"{x.FirstName} {x.LastName}".Trim(),
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                JobTitle = x.JobTitle,
                IsPrimary = x.IsPrimary
            }).ToList(),
            RecentInvoices = customer.Invoices.OrderByDescending(i => i.InvoiceDate).Take(5)
                .Select(i => GetInvoicesQueryHandler.MapToDto(i, customer.Name)).ToList(),
            RecentPayments = customer.Payments.OrderByDescending(p => p.PaymentDate).Take(5)
                .Select(p => GetPaymentsQueryHandler.MapToDto(p, customer.Name, null)).ToList()
        };

        return detail;
    }
}
