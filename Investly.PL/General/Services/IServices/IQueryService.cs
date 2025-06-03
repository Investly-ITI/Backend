using Investly.DAL.Helper;
using System.Linq.Expressions;

namespace Investly.PL.General.Services.IServices
{
    public interface IQueryService<T> where T : class
    {

        public Task<PaginatedResult<T>> FindAllAsync(
            int? take = 10,
            int? skip = 0,
            Expression<Func<T, bool>> criteria = null,
            Expression<Func<T, object>> orderBy = null,
            string orderByDirection = OrderBy.Ascending,
            string? properties = null
        );



    }
}
