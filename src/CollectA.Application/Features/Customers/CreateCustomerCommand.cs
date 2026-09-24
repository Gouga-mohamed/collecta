using CollectA.Application.Common.Extensions;
using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Dtos;
using CollectA.Domain.Entities;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class CreateCustomerCommand : IRequest<CustomerDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeRegister { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Algeria";
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public string? Notes { get; set; }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateCustomerCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Customers.ForTenant(_tenantContext).AnyAsync(c => c.Code == request.Code, cancellationToken))
        {
            throw new InvalidOperationException($"Customer code '{request.Code}' already exists.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            LegalName = request.LegalName,
            TradeRegister = request.TradeRegister,
            TaxId = request.TaxId,
            Industry = request.Industry,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Website = request.Website,
            PaymentTermsDays = request.PaymentTermsDays,
            CreditLimit = request.CreditLimit,
            Notes = request.Notes,
            IsActive = true
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return GetCustomersQueryHandler.MapToDto(customer);
    }
}
