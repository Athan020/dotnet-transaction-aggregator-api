using System;
using System.ComponentModel.DataAnnotations;

namespace Transaction.Aggregator.Api.Dtos;

public sealed record TransactionQueryDto
{
    public string? SourceName { get; init; }

    [Range(1, 50, ErrorMessage = "PageNumber must be greater than 0 and less than or equal to 50.")]
    public int PageNumber { get; init; } = 1;


    [Range(1, 100, ErrorMessage = "PageSize must be greater than 0 and less than or equal to 100.")]
    public int PageSize { get; init; } = 20;

    public DateTimeOffset FromDate { get; init; } = DateTimeOffset.UtcNow.AddMonths(-3);

    public DateTimeOffset ToDate { get; init; } = DateTimeOffset.UtcNow;
}


public static class TransactionQueryDtoExtensions
{
    extension(TransactionQueryDto transactionQueryDto)
    {
        public Domain.Models.TransactionQuery ToTransactionQueryDomainModel()
        {
            return new Domain.Models.TransactionQuery
            {
                SourceName = transactionQueryDto.SourceName,
                PageNumber = transactionQueryDto.PageNumber,
                PageSize = transactionQueryDto.PageSize,
                FromDate = transactionQueryDto.FromDate,
                ToDate = transactionQueryDto.ToDate
            };
        }
    }
}