using Scheduly.Domain.Entities;

namespace Scheduly.Application.Common.Interfaces;

public interface IErrorLogService
{
    Task LogAsync(ErrorLog errorLog);
}
