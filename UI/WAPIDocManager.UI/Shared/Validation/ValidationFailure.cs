using WAPIDocManager.UI.Shared.Forms;

namespace WAPIDocManager.UI.Shared.Validation;

/// <summary>
/// Errore di validazione riferito a una proprietà di una specifica istanza del grafo del modello
/// </summary>
/// <remarks>
/// Istanza + nome proprietà corrispondono al <c>FieldIdentifier</c> di Blazor: <c>Shared/Forms/ModelGraphValidator</c>
/// li usa per associare il messaggio al campo giusto anche dentro le collezioni (es. la riga N del documento).
/// </remarks>
/// <param name="Instance">Oggetto che contiene la proprietà non valida</param>
/// <param name="MemberName">Nome della proprietà</param>
/// <param name="MessageKey">Chiave del messaggio (<see cref="ValidationKeys"/>)</param>
public sealed record ValidationFailure(
    object Instance,
    string MemberName,
    string MessageKey);
