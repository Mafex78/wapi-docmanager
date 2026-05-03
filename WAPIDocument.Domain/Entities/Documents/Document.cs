using Shared.Domain;
using WAPIDocument.Domain.Entities.TaxEntities;

namespace WAPIDocument.Domain.Entities.Documents;

public class Document : IEntity<string>, IDocument, IAudit
{
    public string Id { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
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
    
    public void Create(DocumentType documentType)
    {
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

    public void Delete()
    {
        if(!CanDelete())
        {
            throw new InvalidOperationException($"Cannot delete a document in status {Status}.");
        }
    }

    public Document GenerateFrom(DocumentType targetDocumentType)
    {
        Document newDocument = Clone();
        newDocument.Create(targetDocumentType);

        newDocument.Attach(Id, DocumentLinkType.System);
        
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

    public void Attach(string idToAttach, DocumentLinkType linkType)
    {
        DocumentLink? attachment = LinkedDocuments
            .FirstOrDefault(l => l.TargetDocumentId == idToAttach);

        if (attachment is not null)
        {
            throw new InvalidOperationException($"Document already attached.");
        }
        
        LinkedDocuments.Add(new DocumentLink()
        {
            TargetDocumentId = idToAttach,
            Type = linkType,
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

    private bool CanDelete()
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