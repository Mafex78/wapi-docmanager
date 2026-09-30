namespace WAPIDocManager.UI.Features.Auth.Models;

/// Modello del form di accesso. Non porta attributi di validazione: le regole stanno nel validatore.
public class LoginModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}
