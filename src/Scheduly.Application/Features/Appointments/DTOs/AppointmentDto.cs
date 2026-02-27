using Scheduly.Application.Features.Transactions.DTOs;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Appointments.DTOs;

public record AppointmentDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid UserId,
    string UserName,
    Guid ServiceId,
    string ServiceName,
    Guid? VacancyId,
    int PriceInCents,
    DateTime StartTime,
    DateTime EndTime,
    string? Notes,
    string Status,
    DateTime CreatedAt,
    TransactionDto? Transaction)
{
    public static AppointmentDto FromEntity(Appointment a, Transaction? t = null) => new(
        a.Id, a.CustomerId, a.Customer?.FullName ?? "",
        a.UserId, a.User?.FullName ?? "",
        a.ServiceId, a.Service?.Name ?? "",
        a.VacancyId, a.PriceInCents,
        a.StartTime, a.EndTime, a.Notes,
        a.Status.ToString(), a.CreatedAt,
        t != null ? TransactionDto.FromEntity(t) : null);
}
