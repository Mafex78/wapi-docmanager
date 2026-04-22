using Shared.Domain;
using WAPIDocument.Domain.Entities.TaxEntities;

namespace WAPIDocument.Domain.Entities.Documents;

public class Document : IEntity<string>, IDocument
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public long Version { get; set; }
    public string? Number { get; set; }
    public DateTime Date { get; set; }
    public Customer? Customer { get; set; }
    public string? Currency { get; set; }
    public DocumentType Type { get; protected set; }
    public DocumentStatus Status { get; protected set; } = DocumentStatus.Draft;

    private IList<DocumentLine>? _documentLines = new List<DocumentLine>();
    public IList<DocumentLine>? DocumentLines
    {
        get => _documentLines ?? new List<DocumentLine>();
        set
        {
            _documentLines = value;
            Recalculate();
        }
    }

    public decimal Total { get; private set; }
    public IList<DocumentLink> LinkedDocuments { get; private set; } = new List<DocumentLink>();
    
    public void Setup(DocumentType documentType)
    {
        Id = Guid.NewGuid().ToString();
        Number = Guid.NewGuid().ToString();
        Type = documentType;
        Status = DocumentStatus.Draft;
        var now = DateTime.UtcNow;
        Date = new DateTime(now.Year, now.Month, now.Day, 0, 0,0,  DateTimeKind.Utc);
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Update(
        DateTime documentDate,
        string? currency,
        Customer? customer,
        IList<DocumentLine>? documentLines)
    {
        if(!CanUpdate())
        {
            throw new InvalidOperationException($"Cannot edit a document in status {Status}.");
        }
        
        Date = new  DateTime(documentDate.Year, documentDate.Month, documentDate.Day, 0, 0,0,  DateTimeKind.Utc);
        Currency = currency;
        Customer = customer;
        DocumentLines = documentLines;
        UpdatedAtUtc = DateTime.UtcNow;

        if (Status == DocumentStatus.Ready)
        {
            // Se la modifica viene fatta su un documento Ready
            // allora il documento deve rimanere valido
            Validate(Status);
        }
    }

    public Document GenerateFrom(DocumentType targetDocumentType)
    {
        Document newDocument = Clone();
        newDocument.Setup(targetDocumentType);

        newDocument.LinkedDocuments.Add(new DocumentLink
        {
            TargetDocumentId = Id,
            Type = DocumentLinkType.System, 
            DocumentType = Type
        });
        
        return newDocument;
    }

    public void UpdateStatus(DocumentStatus newStatus)
    {
        switch (newStatus)
        {
            case DocumentStatus.Ready: 
                MarkReady(); 
                break;
            case DocumentStatus.Sent: 
                Send(); 
                break;
            case DocumentStatus.Approved: 
                Approve(); 
                break;
            case DocumentStatus.Rejected: 
                Reject(); 
                break;
            default: throw new ArgumentException($"Unknown target status: {newStatus}.");
        }
    }

    public void Attach(string idToAttach, DocumentType typeToAttach)
    {
        LinkedDocuments.Add(new DocumentLink()
        {
            TargetDocumentId = idToAttach,
            Type = DocumentLinkType.User,
            DocumentType = typeToAttach,
        });
    }

    #region Private methods

    private Document Clone()
    {
        return new Document()
        {
            Id = string.Empty, // reset per sicurezza
            CreatedAtUtc = CreatedAtUtc,
            UpdatedAtUtc = UpdatedAtUtc,
            Number = string.Empty, // reset per sicurezza
            Date = Date,
            Customer = Customer?.Clone(),
            Currency = Currency,
            Type = Type,
            Status = Status,
            DocumentLines = DocumentLines is not null && DocumentLines.Any()
                ? DocumentLines
                    .Select(l => l.Clone())
                    .ToList()
                : new List<DocumentLine>(),
            LinkedDocuments = new  List<DocumentLink>()
        };
    }
    
    private void MarkReady()
    {
        SetStatus(DocumentStatus.Draft, DocumentStatus.Ready);
    }

    private void Send()
    {
        SetStatus(DocumentStatus.Ready, DocumentStatus.Sent);
    }

    private void Approve()
    {
        SetStatus(DocumentStatus.Sent, DocumentStatus.Approved);
    }

    private void Reject()
    {
        SetStatus(DocumentStatus.Sent, DocumentStatus.Rejected);
    }

    private void SetStatus(DocumentStatus requiredStatus, DocumentStatus targetStatus)
    {
        RequireStatus(requiredStatus);
        
        Validate(targetStatus);

        Status = targetStatus;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private void Validate(DocumentStatus targetStatus)
    {
        if (string.IsNullOrWhiteSpace(Currency))
        {
            throw new InvalidOperationException($"Cannot mark document as {targetStatus} without a currency.");
        }

        if (string.IsNullOrWhiteSpace(Customer?.Name) || string.IsNullOrWhiteSpace(Customer?.VatNumber))
        {
            throw new InvalidOperationException($"Cannot mark document as {targetStatus} without a customer.");
        }
            
        if(_documentLines is null || !_documentLines.Any())
        {
            throw new InvalidOperationException($"Cannot mark document as {targetStatus} without any lines.");
        }

        foreach (DocumentLine line in _documentLines)
        {
            if (!line.IsValid())
            {
                throw new InvalidOperationException($"Cannot mark document as {targetStatus} with invalid lines.");
            }
        }
    }

    private void RequireStatus(DocumentStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Operation requires status {expected}, current is {Status}.");
    }
    
    private bool CanUpdate()
    {
        return Status == DocumentStatus.Draft ||
               Status == DocumentStatus.Ready;
    }

    private void Recalculate()
    {
        if (_documentLines is not null &&
            _documentLines.Any())
        {
            Total = Math.Round(_documentLines.Sum(x => x.Total), 2);
        }
    }
    
    #endregion
}