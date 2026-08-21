using EnergyShare_v3.Application.Features.Geocoding;
using EnergyShare_v3.Application.Interfaces;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace EnergyShare_v3.Infrastructure.Services
{
    // Service concret de géocodage utilisant UrbIS.
    // Cette classe se trouve dans Infrastructure car elle dépend d'un service externe HTTP.
    // Elle implémente l'interface IGeocodingService définie dans Application.
    public class UrbisGeocodingService(HttpClient httpClient) : IGeocodingService
    {
        // HttpClient permet d'envoyer des requêtes HTTP vers le service UrbIS.
        // Il est injecté par le mécanisme de Dependency Injection de .NET.
        private readonly HttpClient _httpClient = httpClient;

        // Méthode principale du service  : elle reçoit une adresse et un code postal, interroge UrbIS
        // et retourne les coordonnées géographiques correspondantes.
        //
        // GeocodingResult? signifie que la méthode peut retourner null SI l'adresse est invalide ou si UrbIS ne trouve aucun résultat.
        public async Task<GeocodingResult?> GeocodeAsync(
            string adresseLine1,
            string codePostal,
            CancellationToken cancellationToken = default)
        {
            //Console.WriteLine(">>> GEOCODING SERVICE APPELE <<<");
            //Console.WriteLine($"Adresse reçue = {adresseLine1}");
            //Console.WriteLine($"Code postal reçu = {codePostal}");


            // Première sécurité :   on ne fait pas d'appel vers UrbIS si l'adresse ou le code postal ne sont pas renseignés.
            if (string.IsNullOrWhiteSpace(adresseLine1) || string.IsNullOrWhiteSpace(codePostal))
            {
                return null;
            }

            if (!IsBrusselsPostalCode(codePostal)) //On ne considère que les codes postaux de Bruxelles pour le géocodage. UrbIS ne gère pas les autres codes postaux.
            {
                return null;
            }


            // Le formulaire actuel de l'application contient l'adresse dans une seule chaîne, ex : "Avenue des Arts 21"
            // Or le service UrbIS attend séparément :
            // - le nom de la rue
            // - le numéro de police
            //
            // On utilise donc une expression régulière (Regex) pour essayer de séparer automatiquement les deux parties.
            var match = Regex.Match( adresseLine1.Trim(),  @"^(?<street>.+?)\s+(?<number>\d+[A-Za-z]?)$");
            // Si l'adresse ne correspond pas au format attendu (ex : si aucun numéro n'est trouvé à la fin)le géocodage est abandonné.
            if (!match.Success) {
                //Console.WriteLine( $"Impossible de séparer la rue et le numéro depuis : {adresseLine1}");
                return null;

            }
               

            // Récupération du nom de rue capturé par la Regex.Ex : "Avenue des Arts 21"  devient street = "Avenue des Arts"
            var street = match.Groups["street"].Value.Trim();

            // Récupération du numéro capturé par la Regex. Ex: "Avenue des Arts 21" devient number = "21"
            var number = match.Groups["number"].Value.Trim();

            //Console.WriteLine($"Rue extraite = {street}");
            //Console.WriteLine($"Numéro extrait = {number}");
            //Console.WriteLine($"Code postal = {codePostal}");

            // EPSG:4326 permet d'obtenir des coordonnées sous forme longitude / latitude.

            // UrbIS attend les données de recherche dans un objet JSON
            // transmis dans le paramètre "json" de la requête GET.

            var payload = new
            {
                language = "fr",
                address = new
                {
                    street = new
                    {
                        name = street,
                        postcode = codePostal
                    },
                    number = number
                },
                spatialReference = "4326"
            };

            var json = JsonSerializer.Serialize(payload);  // Transformation de l'objet C# en chaîne JSON.

            // Encodage du JSON afin qu'il puisse être transmis correctement dans l'URL.
            var url =
                $"localization/Rest/Localize/getxycoord" +
                $"?json={Uri.EscapeDataString(json)}" +
                $"&callback=callback";

            // Uri.EscapeDataString encode correctement les valeurs dans une URL.Par ex : "Avenue des Arts"
            // contient des espaces qui doivent être encodés avant  d'être envoyés dans une requête HTTP.


            // Envoi de la requête HTTP GET vers UrbIS --> GetFromJsonAsync :
            // 1. appelle l'URL,
            // 2. récupère la réponse JSON,
            // 3. transforme automatiquement le JSON en objet C# de type UrbisResponse.
            //
            // Le CancellationToken permet d'annuler proprement la requête si le user  quitte la page ou si l'opération est interrompue.
            //var response = await _httpClient.GetFromJsonAsync<UrbisResponse>(url, cancellationToken);
            var httpResponse = await _httpClient.GetAsync( url, cancellationToken);

            //Console.WriteLine($"UrbIS URL : {httpResponse.RequestMessage?.RequestUri}");
            //Console.WriteLine($"UrbIS STATUS : {(int)httpResponse.StatusCode} {httpResponse.StatusCode}");

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                //Console.WriteLine($"UrbIS ERROR RESPONSE : {errorContent}");

                return null;
            }


            var rawJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            Console.WriteLine($"UrbIS RESPONSE : {rawJson}");

            // UrbIS retourne du JSONP, ex :callback({"result":{...},"error":false,"status":"success"})
            // Il faut retirer l'enveloppe callback(...) pour récupérer le JSON pur.

            var startIndex = rawJson.IndexOf('(');
            var endIndex = rawJson.LastIndexOf(')');

            if (startIndex < 0 || endIndex <= startIndex)
            {
                //Console.WriteLine("Réponse UrbIS au format inattendu.");
                return null;
            }

            var jsonResponse = rawJson.Substring(
                startIndex + 1,
                endIndex - startIndex - 1);

            // Désérialisation du JSON pur en objet C#.
            var response = JsonSerializer.Deserialize<UrbisResponse>(
                jsonResponse,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // Vérification de la réponse --> On retourne null si :
            // - aucune réponse n'a été reçue,
            // - UrbIS indique une erreur,
            // - aucun point géographique n'a été trouvé.
            
            if (response is null)
            {
                //Console.WriteLine("Impossible de désérialiser la réponse UrbIS.");
                return null;
            }

            if (response.Error)
            {
                //Console.WriteLine($"UrbIS indique une erreur. Status = {response.Status}");
                return null;
            }

            if (response.Result?.Point is null)
            {
                //Console.WriteLine("UrbIS n'a retourné aucun point géographique.");
                return null;
            }

            const double MinimumMatchScore = 5.0;   //seuil minimal de score pour considérer le résultat comme valide. UrbIS retourne un score de 0 à 10.

            if (response.Result.MatchScore < MinimumMatchScore)
            {
                return null;
            }

            // Transformation de la réponse UrbIS en objet métier/application GeocodingResult.
            // Dans la réponse WGS84 testée : X = longitude et Y = latitude
            // Ex : x = 4.369348...  et  y = 50.846110...
            // On fait donc attention à ne pas inverser les deux valeurs!

            Console.WriteLine(
                $"Coordonnées trouvées : Lat={response.Result.Point.Y}, " +
                $"Lon={response.Result.Point.X}, " +
                $"Score={response.Result.MatchScore}");

            return new GeocodingResult(
                Latitude: response.Result.Point.Y,
                Longitude: response.Result.Point.X,
                MatchScore: response.Result.MatchScore
            );
        }


        // Classes internes utilisées uniquement pour désérialiser la structure JSON retournée par UrbIS.
        // Elles sont privées car le reste de l'application  n'a pas besoin de connaître la structure technique d'UrbIS.
        private class UrbisResponse {
            public UrbisResult? Result { get; set; }   // Contient le résultat principal retourné par UrbIS.
            public bool Error { get; set; }  // Indique si UrbIS signale une erreur.

            public string? Status { get; set; }  // Exemple de valeur : "success"
        }


        private class UrbisResult {
            public UrbisPoint? Point { get; set; } // Coordonnées géographiques retournées.

            // Score fourni par UrbIS indiquantla qualité du rapprochement entre l'adresse recherchée et l'adresse trouvée dans le référentiel.
            public double MatchScore { get; set; }
        }

        private class UrbisPoint  {   public double X { get; set; } // En WGS84 : X correspond à la longitude.
            public double Y { get; set; } // En WGS84 :Y correspond à la latitude.
        }

        private static bool IsBrusselsPostalCode(string codePostal)
        {
            return codePostal is
                "1000" or "1020" or "1030" or "1040" or "1050" or
                "1060" or "1070" or "1080" or "1081" or "1082" or
                "1083" or "1090" or "1120" or "1130" or "1140" or
                "1150" or "1160" or "1170" or "1180" or "1190" or
                "1200" or "1210";
        }
    }
}
