// Preferenze del browser — tema e lingua — lette e scritte dall'applicazione.
// Sono salvate nell'archivio permanente perché appartengono al browser e non all'utente: cambiando utente sulla
// stessa macchina restano, mentre la sessione di accesso, che appartiene all'utente, sta in un archivio che si
// svuota chiudendo la scheda.
// La chiave del tema deve coincidere con quella usata dallo script che applica il tema al caricamento della pagina,
// prima che l'applicazione parta: è quello a evitare il lampeggio di tema chiaro su chi ha scelto lo scuro. Per la
// stessa ragione questo file viene caricato prima dell'applicazione.
// Ogni accesso all'archivio è protetto, perché il browser può negarlo: succede in navigazione privata o con i dati
// dei siti disabilitati, e in quel caso l'applicazione deve funzionare comunque, con le impostazioni predefinite.
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
