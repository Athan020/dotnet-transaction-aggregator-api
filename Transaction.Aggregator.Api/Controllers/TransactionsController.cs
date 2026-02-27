using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Transaction.Aggregator.Api.Dtos;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Api.Controllers
{
    [Route("transactionmanagement/v1/transactions")]
    [ApiController]
    [Consumes("application/json")]
    [Produces("application/json")]
    [EnableRateLimiting("Fixed")]
    public class TransactionsController(ITransactionManager transactionAggregator) : ControllerBase
    {
        private readonly ITransactionManager _transactionAggregator = transactionAggregator;

        [HttpGet]
        [ProducesResponseType<PaginatedResult<TransactionItem>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionsAsync([FromQuery] TransactionQueryDto query, CancellationToken cancellationToken)
        {
            var queryModel = query.ToTransactionQueryDomainModel();

            var transactions = await _transactionAggregator.GetTransactionsAsync(queryModel, cancellationToken);

            if (transactions is null || transactions.Items is { Count: 0 })
            {
                return NoContent();
            }

            return Ok(transactions);
        }

    }
}
