using FinLedger.Accounts.Application.Abstractions;
using FinLedger.Accounts.Domain.Entities;
using FinLedger.Accounts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinLedger.Accounts.Infrastructure.Services;

public sealed class AccountNumberGenerator : IAccountNumberGenerator
{
    private const string Prefix = "FL";

    private readonly AccountsDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AccountNumberGenerator(AccountsDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var sequence = await _dbContext.AccountNumberSequences.FirstOrDefaultAsync(x => x.Prefix == Prefix, cancellationToken);
        if (sequence is null)
        {
            sequence = AccountNumberSequence.Create(Prefix, _dateTimeProvider.UtcNow);
            await _dbContext.AccountNumberSequences.AddAsync(sequence, cancellationToken);
        }

        return sequence.GenerateNext(_dateTimeProvider.UtcNow);
    }
}
