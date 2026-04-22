namespace Shared.Application.Dto;

public record FilterPagingDto : IFilterPagingDto
{
    /// <summary>
    /// Pagina da restituire
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Numero di elementi da restituire
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Entità per la quale effettuare l'ordinamento dei risultati, opzionale
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Direzione ordinamento (ASC o DESC), opzionale se SortBy è nullo
    /// </summary>
    public string? SortDirection { get; set; }

    /// <summary>
    /// Testo libero da ricercare, opzionale
    /// </summary>
    public string? PlainText { get; set; }
}