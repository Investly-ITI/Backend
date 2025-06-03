using Investly.DAL.Entities;
using Investly.DAL.Helper;
using Investly.DAL.Repos.IRepos;
using Investly.PL.General.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Investly.PL.General
{
    public class QueryService<T> : IQueryService<T> where T : class
    {
        private readonly IRepo<T> _repo;
        private readonly AppDbContext _context;

        public QueryService(IRepo<T> repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }
        public async Task<PaginatedResult<T>> FindAllAsync(
            int? take = 10,
            int? skip = 0,
            Expression<Func<T, bool>> criteria = null,
            Expression<Func<T, object>> orderBy = null,
            string orderByDirection = OrderBy.Ascending,
            string? properties = null
        )
        {
            // Step 1: Start with filtered + included query
            IQueryable<T> query = _repo.FindAll(criteria, properties);

            // Step 2: Get total count of the whole table (not filtered)
            var totalItemsInTable = await _context.Set<T>().CountAsync();

            // Step 3: Get count of filtered items
            var totalFilteredItems = await query.CountAsync();

            // Step 4: Apply ordering if provided
            if (orderBy != null)
            {
                query = orderByDirection == OrderBy.Ascending
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);
            }

            // Step 5: Apply pagination
            if (skip.HasValue && skip.Value > 0)
                query = query.Skip(skip.Value);

            if (take.HasValue && take.Value > 0)
                query = query.Take(take.Value);

            // Step 6: Execute query and fetch results
            var items = await query.ToListAsync();

            // Step 7: Calculate pagination metadata
            var pageSize = take.GetValueOrDefault(10);
            var currentPage = (skip.GetValueOrDefault(0) / pageSize) + 1;
            var totalPages = (int)Math.Ceiling(totalFilteredItems / (double)pageSize);

            return new PaginatedResult<T>
            {
                Items = items,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalFilteredItems = totalFilteredItems,
                TotalItemsInTable = totalItemsInTable
            };
        }

    }
}
