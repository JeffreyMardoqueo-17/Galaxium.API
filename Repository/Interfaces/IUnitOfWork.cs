using System;
using System.Threading.Tasks;

namespace Galaxium.Api.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        Task ExecuteInTransactionAsync(Func<Task> action);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    }
}
