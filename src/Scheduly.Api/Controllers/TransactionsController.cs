using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Transactions.Commands.CancelTransaction;
using Scheduly.Application.Features.Transactions.Commands.PayTransaction;
using Scheduly.Application.Features.Transactions.Queries.GetTransactionById;
using Scheduly.Application.Features.Transactions.Queries.GetTransactions;
using Scheduly.Application.Features.Transactions.Queries.GetTransactionSummary;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class TransactionsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetTransactionsQuery(from, to, status, customerId, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTransactionById(Guid id)
    {
        var result = await Mediator.Send(new GetTransactionByIdQuery(id));
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetTransactionSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var result = await Mediator.Send(new GetTransactionSummaryQuery(from, to));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/pay")]
    public async Task<IActionResult> PayTransaction(Guid id, [FromBody] PayTransactionRequest request)
    {
        var result = await Mediator.Send(new PayTransactionCommand(id, request.PaymentMethod));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTransaction(Guid id)
    {
        await Mediator.Send(new CancelTransactionCommand(id));
        return NoContent();
    }
}

public record PayTransactionRequest(string PaymentMethod);
