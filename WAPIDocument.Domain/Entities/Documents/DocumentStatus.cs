namespace WAPIDocument.Domain.Entities.Documents;

public enum DocumentStatus
{    
    Draft = 0,      // incompleto
    Ready = 1,      // completo (tutti i campi obbligatori presenti)
    Sent = 2,       // inviato al cliente
    Approved = 3,   // approvato
    Rejected = 4    // rifiutato
}