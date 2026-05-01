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

        [HttpGet("{accountId:long}")]
        [ProducesResponseType<PaginatedResult<TransactionItem>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTransactionsAsync(long accountId, [FromQuery] TransactionQueryDto query, CancellationToken cancellationToken)
        {
            var queryModel = query.ToTransactionQueryDomainModel();

            if(queryModel.FromDate > queryModel.ToDate)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid date range",
                    Detail = "FromDate cannot be greater than ToDate.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if(queryModel.FromDate < DateTimeOffset.UtcNow.AddYears(-1))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid FromDate",
                    Detail = "FromDate cannot be older than 1 year from the current date.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if(queryModel.ToDate > DateTimeOffset.UtcNow)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid ToDate",
                    Detail = "ToDate cannot be in the future.",
                    Status = StatusCodes.Status400BadRequest
                });
            }


            queryModel = queryModel with { AccountId = accountId };

            var transactions = await _transactionAggregator.GetTransactionsAsync(queryModel, cancellationToken);

            if (transactions is null || transactions.Items is { Count: 0 })
            {
                return NoContent();
            }

            return Ok(transactions);
        }

    }
}
