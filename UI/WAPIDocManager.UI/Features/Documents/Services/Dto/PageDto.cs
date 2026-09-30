namespace WAPIDocManager.UI.Features.Documents.Services.Dto;

/// Pagina di risultati così come arriva dal server.
/// È l'unica forma di trasporto senza operatore di conversione: il linguaggio non permette di dichiararne uno fra
/// due tipi generici come questi, quindi la conversione è scritta a mano nel servizio che esegue la ricerca.
public record PageDto<T>
{
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public List<T>? Items { get; init; }
}
