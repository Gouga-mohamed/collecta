using CollectA.Application.Common.Dtos;
using CollectA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CollectA.Application.Features.Customers;

public class UpdateCustomerCommand : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeRegister { get; set; }
    public string? TaxId { get; set; }
    public string? Industry { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly IIIApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IIIApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Include(c => c.Invoices)
            .Include(c => c.RiskScores.OrderByDescending(r => r.CalculatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (customer == null) throw new KeyNotFoundException($"Customer '{request.Id}' not found.");

        customer.Name = request.Name;
        customer.LegalName = request.LegalName;
        customer.TradeRegister = request.TradeRegister;
        customer.TaxId = request.TaxId;
        customer.Industry = request.Industry;
        customer.Address = request.Address;
        customer.City = request.City;
        customer.Country = request.Country;
        customer.PhoneNumber = request.PhoneNumber;
        customer.Email = request.Email;
        customer.Website = request.Website;
        customer.PaymentTermsDays = request.PaymentTermsDays;
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;
        customer.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return GetCustomersQuery.MapToDto(customer);
    }
}
