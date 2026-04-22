namespace WAPIDocument.Application.Dto.Document;

public record DocumentUpdateRequest
{
    public DateTime Date { get; set; }
    public string? Currency { get; set; }
    public CustomerDto? Customer { get; set; }
    public IList<DocumentCreateUpdateRequestDocumentLine>? DocumentLines { get; set; } =  new List<DocumentCreateUpdateRequestDocumentLine>();
}