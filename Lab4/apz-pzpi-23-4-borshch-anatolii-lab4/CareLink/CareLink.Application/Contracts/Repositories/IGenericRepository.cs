namespace CareLink.Application.Contracts.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        public Task<IReadOnlyCollection<T>> GetAllAsync();
        public Task<T?> GetByIdAsync(long id);
        public Task AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(T entity);
    }
}