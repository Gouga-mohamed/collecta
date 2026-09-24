using CollectA.Domain.Common.Interfaces;
using CollectA.Application.Common.Interfaces;
using CollectA.Domain.Entities;
using CollectA.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CollectA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditService _auditService;

    public CustomersController(ApplicationDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    private Guid GetTenantId()
    {
        var claim = User.FindFirstValue("tenant_id");
        return Guid.Parse(claim!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var tenantId = GetTenantId();
        var customers = await _context.Customers
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomer(Guid id)
    {
        var tenantId = GetTenantId();
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
    {
        var tenantId = GetTenantId();
        customer.TenantId = tenantId;
        customer.Id = Guid.NewGuid();

        if (await _context.Customers.AnyAsync(c => c.Code == customer.Code && c.TenantId == tenantId))
        {
            return BadRequest("Customer code already exists.");
        }

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Create", nameof(Customer), customer.Id, null, null);

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, Customer customer)
    {
        var tenantId = GetTenantId();
        var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (existing == null) return NotFound();

        existing.Name = customer.Name;
        existing.LegalName = customer.LegalName;
        existing.TradeRegister = customer.TradeRegister;
        existing.TaxId = customer.TaxId;
        existing.Industry = customer.Industry;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.Country = customer.Country;
        existing.PhoneNumber = customer.PhoneNumber;
        existing.Email = customer.Email;
        existing.Website = customer.Website;
        existing.PaymentTermsDays = customer.PaymentTermsDays;
        existing.CreditLimit = customer.CreditLimit;
        existing.IsActive = customer.IsActive;
        existing.Notes = customer.Notes;

        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Update", nameof(Customer), id, null, null);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var tenantId = GetTenantId();
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        await _auditService.LogAsync("Delete", nameof(Customer), id, null, null);

        return NoContent();
    }
}
