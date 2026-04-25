window.energyShareNetworkStatus = {
    update: function (isOnline) {
        const badge = document.getElementById("network-status-badge");

        if (!badge) {
            console.warn("Badge réseau introuvable");
            return;
        }

        badge.classList.remove("online", "offline");

        if (isOnline) {
            badge.classList.add("online");
            badge.textContent = "En ligne";
        } else {
            badge.classList.add("offline");
            badge.textContent = "Hors connexion";
        }
    },

    init: function () {
        window.addEventListener("online", () => {
            window.energyShareNetworkStatus.update(true);
        });

        window.addEventListener("offline", () => {
            window.energyShareNetworkStatus.update(false);
        });

        window.energyShareNetworkStatus.update(navigator.onLine);
    }
};

document.addEventListener("DOMContentLoaded", function () {
    window.energyShareNetworkStatus.init();
});