using Scheduly.Domain.Common;

namespace Scheduly.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = [];
    public ICollection<Customer> Customers { get; set; } = [];
    public ICollection<Appointment> Appointments { get; set; } = [];
    public ICollection<Service> Services { get; set; } = [];
    public ICollection<Vacancy> Vacancies { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
