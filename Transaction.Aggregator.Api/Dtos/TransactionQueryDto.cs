using System;
using System.ComponentModel.DataAnnotations;

namespace Transaction.Aggregator.Api.Dtos;

public sealed record TransactionQueryDto
{
    [Range(0, long.MaxValue, ErrorMessage = "AccountId must be a valid account identifier.")]
    public long? AccountId { get; init; }

    public string? SourceName { get; init; }

    [Range(1, 50, ErrorMessage = "PageNumber must be greater than 0 and less than or equal to 50.")]
    public int PageNumber { get; init; } = 1;


    [Range(1, 100, ErrorMessage = "PageSize must be greater than 0 and less than or equal to 100.")]
    public int PageSize { get; init; } = 20;
}


public static class TransactionQueryDtoExtensions
{
    public static Domain.Models.TransactionQuery ToTransactionQueryDomainModel(this TransactionQueryDto transactionQueryDto)
    {
        return new Domain.Models.TransactionQuery
        {
            AccountId = transactionQueryDto.AccountId ?? 0,
            SourceName = transactionQueryDto.SourceName,
            PageNumber = transactionQueryDto.PageNumber,
            PageSize = transactionQueryDto.PageSize
        };
    }
}