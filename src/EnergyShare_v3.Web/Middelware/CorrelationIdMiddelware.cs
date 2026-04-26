namespace EnergyShare_v3.Web.Middleware;


// but : tracer une requête de bout en bout (logs, tracabilité, etc.) en utilisant un ID de corrélation
// unique pour chaque requête HTTP --> Tout les logs liés à cette requête auront le même ID de corrélation,
// ce qui facilite le suivi et le débogage
public sealed class CorrelationIdMiddelware    // sealed : on ne veut pas que d'autres classes héritent de ce middleware, car il est conçu pour une fonction spécifique et ne devrait pas être étendu ou modifié par héritage
{
    public const string HeaderName = "X-Correlation-ID";  //nom standard du header HTTP utilisé pour transmettre l'ID de corrélation entre les services. 

    private readonly RequestDelegate _next;       // Représente le prochain middleware dans la pipeline de traitement des requêtes HTTP. Le middleware doit appeler _next(context) pour passer la requête au middleware suivant après avoir effectué son travail.
    private readonly ILogger<CorrelationIdMiddelware> _logger; //système de log injecté par DI pour enregistrer des informations sur les requêtes, y compris l'ID de corrélation, ce qui facilite le suivi et le débogage.

    public CorrelationIdMiddelware(   //injection de dépendances pour le middleware, permettant d'obtenir une instance de RequestDelegate pour le prochain middleware et un logger pour enregistrer les informations de corrélation.
        RequestDelegate next,
        ILogger<CorrelationIdMiddelware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {    //etape 1 : Vérifier si la requête entrante contient déjà un ID de corrélation dans les headers. Si oui, utiliser cet ID, sinon en générer un nouveau (GUID).
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existingCorrelationId)
            && !string.IsNullOrWhiteSpace(existingCorrelationId)
                ? existingCorrelationId.ToString()
                : Guid.NewGuid().ToString();
         //étape 2 : ajouter dans la réponse
        context.Response.Headers[HeaderName] = correlationId;
        //étape 3 !!: ajouter au scope de logging pour que tous les logs liés à cette requête incluent l'ID de corrélation, 
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {   //étape 4 : continuer la requête dans la pipeline de traitement des requêtes en appelant le middleware suivant.
            await _next(context);
        }
    }
}
