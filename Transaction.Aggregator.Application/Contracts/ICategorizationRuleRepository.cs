using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Transaction.Aggregator.Domain.Models;

namespace Transaction.Aggregator.Application.Contracts;

public interface ICategorizationRuleRepository
{
    /// <summary>
    ///     Retrieves all enabled categorization rules ordered by priority.
    /// </summary>
    Task<IEnumerable<CategorizationRule>> GetRulesAsync(CancellationToken cancellationToken = default);
}