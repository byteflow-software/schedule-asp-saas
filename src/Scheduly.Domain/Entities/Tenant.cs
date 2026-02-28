using Scheduly.Domain.Common;

namespace Scheduly.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Business data
    public string? CpfCnpj { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? AddressNumber { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? LogoUrl { get; set; }

    // Asaas integration
    public string? AsaasApiKey { get; set; }
    public string? AsaasWalletId { get; set; }

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Service> Services { get; set; } = [];
    public ICollection<Vacancy> Vacancies { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
