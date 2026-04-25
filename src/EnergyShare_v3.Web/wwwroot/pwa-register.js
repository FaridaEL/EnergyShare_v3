window.registerServiceWorker = async function () {  // Fonction globale appelée pour enregistrer le Service Worker

    // Vérifie si le navigateur supporte les Service Workers ( normalement, tous les navigateurs modernes oui, mais c’est une bonne pratique))
    if ("serviceWorker" in navigator) {

        try {
            // Enregistre le fichier service-worker.js auprès du navigateur  -> le service worker devient actif
            await navigator.serviceWorker.register("/service-worker.js", {

                // Empêche le navigateur d'utiliser une version en cache du service worker -->  Permet d’avoir toujours la version la plus récente!
                updateViaCache: "none"
            });

            console.log("Service Worker enregistré");   // Message de confirmation dans la console (F12)

        } catch (error) {

            console.error("Erreur d’enregistrement du Service Worker", error); // En cas d'erreur (ex: mauvais chemin, fichier manquant, HTTPS absent)
        }
    }
};