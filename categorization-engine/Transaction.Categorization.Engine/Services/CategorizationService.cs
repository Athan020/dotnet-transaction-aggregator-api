using System;
using Categorization;
using Grpc.Core;
using static Categorization.Categorization;

namespace Transaction.Categorization.Engine.Services;

public class CategorizationService : CategorizationBase
{

    public override Task<CategorizeTransactionResponse> CategorizeTransaction(CategorizeTransactionRequest request, ServerCallContext context)
    {
        var response = new CategorizeTransactionResponse
        {
            Category = "Example Category"
        };

        return Task.FromResult(response);
    }

    public override Task<CategorizeTransactionsBatchResponse> CategorizeTransactionsBatch(CategorizeTransactionsBatchRequest request, ServerCallContext context)
    {
        // Implement your batch categorization logic here
        var response = new CategorizeTransactionsBatchResponse();
        foreach (var transaction in request.Requests)
        {
            response.Responses.Add(new CategorizeTransactionResponse
            {
                TransactionId = transaction.TransactionId,
                Category = "Example Category"
            });
        }

        return Task.FromResult(response);
    }
}
