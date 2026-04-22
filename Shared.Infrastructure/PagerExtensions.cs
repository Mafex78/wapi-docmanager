using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Shared.Infrastructure;

public static class PagerExtensions
{
    /// <summary>
    /// Paginazione
    /// </summary>
    /// <param name="query">Query da eseguire</param>
    /// <param name="page">Pagina corrente</param>
    /// <param name="pageSize">Dimensione della pagina</param>
    /// <param name="sortBy">Ordinamento per</param>
    /// <param name="sortDirection">Direzione ordinamento</param>
    /// <typeparam name="TModel">Entità</typeparam>
    /// <returns>Oggetto paged</returns>
    public static async Task<PagedList<TModel>> PaginateAsync<TModel>(
        this IQueryable<TModel> query,
        int page,
        int pageSize,
        string? sortBy = null,
        string? sortDirection = null)
        where TModel : class
    {
        var paged = new PagedList<TModel>();
        page = page < 0 ? 1 : page;
        paged.CurrentPage = page;
        paged.PageSize = pageSize;

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (string.IsNullOrEmpty(sortDirection)) sortDirection = "ascending";
            query = query.OrderBy($"{sortBy} {sortDirection}");
        }

        var startRow = (page - 1) * pageSize;
        paged.Items = await query
            .Skip(startRow)
            .Take(pageSize)
            .ToListAsync();

        paged.TotalItems = await query.CountAsync();
        paged.TotalPages = (int)Math.Ceiling(paged.TotalItems / (double)pageSize);

        return paged;
    }
}