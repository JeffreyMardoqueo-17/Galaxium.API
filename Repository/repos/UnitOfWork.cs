using System;
using System.Threading;
using System.Threading.Tasks;
using Galaxium.API.Data;
using Galaxium.Api.Repository.Interfaces;

namespace Galaxium.Api.Repository.repos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GalaxiumDbContext _context;

        public UnitOfWork(GalaxiumDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                state: _context,
                operation: async (_, __, cancellationToken) =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        await action();
                        await transaction.CommitAsync(cancellationToken);
                        return 0;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                },
                verifySucceeded: null,
                cancellationToken: CancellationToken.None);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(
                state: _context,
                operation: async (_, __, cancellationToken) =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        var result = await action();
                        await transaction.CommitAsync(cancellationToken);
                        return result;
                    }
                    catch
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        throw;
                    }
                },
                verifySucceeded: null,
                cancellationToken: CancellationToken.None);
        }
    }
}
