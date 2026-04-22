using Shared.Domain;

namespace Shared.Application.Dto;

/// <summary>
/// Filtro in input per la paginazione
/// </summary>
public interface IFilterPagingDto : IFilterPaging
{
    /// <summary>
    /// Testo libero da ricercare, opzionale
    /// </summary>
    public string? PlainText { get; set; }
}