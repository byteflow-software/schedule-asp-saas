using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Vacancies.Commands.DeleteVacancy;

public class DeleteVacancyCommandHandler : IRequestHandler<DeleteVacancyCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteVacancyCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteVacancyCommand request, CancellationToken cancellationToken)
    {
        var vacancy = await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("Vacancy", request.Id);

        if (vacancy.IsBooked)
            throw new DomainException("VACANCY_BOOKED", "Cannot delete a booked vacancy.");

        _context.Vacancies.Remove(vacancy);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
