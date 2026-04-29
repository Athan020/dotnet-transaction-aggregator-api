using System;
using Categorization;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using static Categorization.Categorization;

namespace Transaction.Categorization.Engine.Services;

public class CategorizationService(TransactionsContext transactionsContext) : CategorizationBase
{

    private readonly TransactionsContext _transactionsContext = transactionsContext;

    public override async Task<CategorizeTransactionResponse> CategorizeTransaction(CategorizeTransactionRequest request, ServerCallContext context)
    {
        var description = request.Description;

        var categorizationRules = await _transactionsContext
                                    .Categories
                                    .Where(c => c.Version == 1)
                                    .Select(c => new Category
                                    {
                                        Id = c.Id,
                                        Name = c.Name,
                                        Description = c.Description,
                                        SubcategoryOf = c.SubcategoryOf,
                                        Keywords = c.Keywords
                                    }).ToListAsync();

        return await CategorizeTransactionAsync(request, description, categorizationRules);
    }

    private static Task<CategorizeTransactionResponse> CategorizeTransactionAsync(CategorizeTransactionRequest request, string description, List<Category> categorizationRules)
    {
        foreach (var category in categorizationRules)
        {
            if (category.Keywords!.Any(k => description.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new CategorizeTransactionResponse
                {
                    Category = category.Name,
                    TransactionId = request.TransactionId,
                    CategoryId = category.Id.ToString()
                });
            }
        }

        var uncategorizedCategoryId = categorizationRules.FirstOrDefault(c => c.Name.Equals("Uncategorized", StringComparison.OrdinalIgnoreCase))?.Id.ToString();

        return Task.FromResult(new CategorizeTransactionResponse
        {
            Category = "Uncategorized",
            TransactionId = request.TransactionId,
            CategoryId = uncategorizedCategoryId ?? "0"
        });
    }

    public override async Task<CategorizeTransactionsBatchResponse> CategorizeTransactionsBatch(CategorizeTransactionsBatchRequest request, ServerCallContext context)
    {
        // Implement your batch categorization logic here
        var response = new CategorizeTransactionsBatchResponse();

        var categorizationRules = await _transactionsContext
                                .Categories
                                .Where(c => c.Version == 1)
                                .Select(c => new Category
                                {
                                    Id = c.Id,
                                    Name = c.Name,
                                    Description = c.Description,
                                    SubcategoryOf = c.SubcategoryOf,
                                    Keywords = c.Keywords
                                }).ToListAsync();

        var categorizationRequests = request.Requests.Select(r => CategorizeTransactionAsync(r, r.Description, categorizationRules));

        var categorizationResults = await Task.WhenAll(categorizationRequests);

        response.Responses.AddRange(categorizationResults);

        return await Task.FromResult(response);
    }



}
