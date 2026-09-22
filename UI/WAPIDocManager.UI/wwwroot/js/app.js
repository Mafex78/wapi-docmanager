// Preferenze utente lato browser (tema e lingua) usate dai componenti Blazor via JS interop
//
// Chiamanti:
// - Shared/Components/ThemeToggle.razor: getTheme / setTheme
// - Shared/Components/CultureSelector.razor: setCulture (seguito dal reload della pagina)
// - Shared/Localization/AppCultures.cs: getCulture / setDocumentLanguage, all'avvio in Program.cs (prima di RunAsync:
//   per questo il file è caricato in index.html prima di blazor.webassembly.js)
// Persistenza nel localStorage (preferenza del browser, non dell'utente): la sessione JWT sta invece nel
// sessionStorage (Shared/Authentication/SessionStorageUserSessionStore). La chiave del tema deve coincidere con
// quella dello script inline di index.html. Gli accessi allo storage sono protetti da try/catch perché il browser
// può bloccarli (es. navigazione privata o cookie disabilitati).
window.wapiPreferences = (function () {
    var themeKey = 'wapidocmanager.theme';
    var cultureKey = 'wapidocmanager.culture';

    function read(key) {
        try { return window.localStorage.getItem(key); } catch (e) { return null; }
    }

    function write(key, value) {
        try { window.localStorage.setItem(key, value); } catch (e) { }
    }

    return {
        getTheme: function () {
            return document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'dark' : 'light';
        },
        setTheme: function (theme) {
            var value = theme === 'dark' ? 'dark' : 'light';
            document.documentElement.setAttribute('data-bs-theme', value);
            write(themeKey, value);
        },
        getCulture: function () {
            return read(cultureKey);
        },
        setCulture: function (culture) {
            write(cultureKey, culture);
        },
        setDocumentLanguage: function (language) {
            document.documentElement.setAttribute('lang', language);
        }
    };
})();
