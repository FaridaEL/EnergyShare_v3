// Nom du cache utilisé par le service worker  --> Si le nom est différent, le navigateur considère que c’est un nouveau cache et va le créer. Cela permet de forcer la mise à jour du cache lorsque on modifie ce fichier.

const CACHE_NAME = "energyshare-cache-v2";
const OFFLINE_URL = "/offline.html";   // Page affichée si aucune connexion réseau n’est disponible

// Liste des ressources à mettre en cache dès l’installation  =  "noyau" de l'application (shell)
const ASSETS_TO_CACHE = [
    "/", // page d’accueil
    "/offline.html", // page fallback hors ligne
    "/manifest.webmanifest", // manifest PWA
    "/favicon.png",
    "/icons/icon-192.png",
    "/icons/icon-512.png",
    "/app.css",
    "/lib/bootstrap/dist/css/bootstrap.min.css"
];

// ==========================
// ÉTAPE 1 : INSTALLATION
// ==========================

self.addEventListener("install", event => {    // Cet événement est déclenché une seule fois lors de l’installation du service worker
    event.waitUntil(   // waitUntil permet d’attendre que le cache soit rempli avant de terminer l’installation
        caches.open(CACHE_NAME) // ouvre (ou crée) le cache
            .then(cache => cache.addAll(ASSETS_TO_CACHE)) // ajoute les fichiers dans le cache
    );
    self.skipWaiting();  // skipWaiting permet d’activer immédiatement ce service worker sans attendre que les anciens onglets soient fermés
});


// ==========================
// ÉTAPE 2 : ACTIVATION
// ==========================

self.addEventListener("activate", event => { // Cet événement est déclenché après l’installation

    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys
                    // On supprime tous les anciens caches
                    .filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            )
        )
    );

    self.clients.claim(); // Permet au service worker de prendre le contrôle immédiatement des pages
});


// ==========================
// ÉTAPE 3 : INTERCEPTION DES REQUÊTES
// ==========================

self.addEventListener("fetch", event => {  // Cet événement intercepte toutes les requêtes réseau de l’application

    const request = event.request;
 
    if (request.method !== "GET") {   // On ne gère que les requêtes GET (pas POST, PUT, etc.)
        return;
    }

    const acceptHeader = request.headers.get("accept") || "";


    // ==========================
    // CAS 1 : NAVIGATION HTML
    // ==========================
    // Ex : ouverture d’une page, navigation entre pages
    /*if (request.mode === "navigate" || acceptHeader.includes("text/html")) {

        event.respondWith(
            fetch(request) // On essaie d’abord le réseau (network first)
                .then(response => {
  
                    const responseClone = response.clone(); // On clone la réponse car elle ne peut être lue qu’une seule fois

                    caches.open(CACHE_NAME)  // On met la page en cache pour une future utilisation offline
                        .then(cache => cache.put(request, responseClone));

                    return response;
                })
                .catch(async () => {

                    const cached = await caches.match(request);  // Si le réseau échoue (offline), on tente de récupérer la page depuis le cache
                    return cached || caches.match(OFFLINE_URL);   // Si pas trouvé → on affiche la page offline
                })
        );

        return;
    } */


    // Stratégie utilisée : network first.
    // On tente toujours d’aller chercher la page sur le serveur.
    // Si le serveur ne répond pas, on affiche directement la page offline.
    // Dans une application Blazor Server, c’est plus sûr que de réafficher une ancienne page HTML en cache,
    // car l’interactivité dépend d’une connexion active au serveur.
    if (request.mode === "navigate" || acceptHeader.includes("text/html")) {

        event.respondWith(
            fetch(request)
                .then(response => {
                    return response;
                })
                .catch(() => {
                    return caches.match(OFFLINE_URL);
                })
        );

        return;
    }


    // ==========================
    // CAS 2 : FICHIERS STATIQUES
    // ==========================
    // Ex : CSS, images, JS
    event.respondWith(
        caches.match(request).then(cached => {

            
            if (cached) {   // Si trouvé dans le cache → on le retourne directement
                return cached;
            } 
            return fetch(request).then(response => {   // Sinon → on va chercher sur le réseau

                const clone = response.clone(); // On clone la réponse pour la stocker
 
                caches.open(CACHE_NAME)   // On ajoute la ressource au cache pour les prochaines fois
                    .then(cache => cache.put(request, clone));

                return response;
            });
        })
    );
});