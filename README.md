# ⚡ EnergyShare
## Description
EnergyShare est une application web développée dans le cadre d’un projet académique.  
EnergyShare vise à faciliter l’accès au partage d’énergie en proposant un module de mise en relation entre acheteurs et vendeurs 
ainsi qu’un module de gestion de partage d’énergie conforme au cadre réglementaire en vigueur à Bruxelles.
## Prérequis
- .NET SDK
- SQL Server ou SQL Server Express
- Visual Studio ou Visual Studio Code
## Lancement local
1. Cloner le dépôt :

```bash
git clone https://github.com/FaridaEL/EnergyShare_v3.git
```
## Comptes de démonstration
Les comptes ci-dessous sont fournis uniquement à des fins de test et de démonstration du MVP.  
Mot de passe de démonstration : `Test1234` 

Administrateur :   `admin.test@example.com`  
Vendeur 1   :      `sarah.dupont@example.com`  
Vendeur 2 :        `julien.martin@example.com`  
Acheteur 1 :       `lea.bernard@example.com`  
Acheteur 2 :       `hugo.lambert@example.com`  
Acheterur 3 :     `contact@boulangerie-dupain.be`  
GRD :             `agent.sibelga@example.com`  

## Stack technique
- ASP.NET Core
- Blazor Server
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT
- MediatR
- FluentValidation
- Ardalis.Result
- Swagger / OpenAPI

## Architecture
Le projet suit une architecture inspirée de la Clean Architecture :
- Domain : entités et règles métier
- Application : cas d’utilisation, DTOs, handlers
- Infrastructure : persistance, services techniques
- Web : interface Blazor Server et endpoints

# Fonctionnalités détaillées

## 🔐 1. Accès à la plateforme et authentification

| ID     | Fonctionnalité                                   | Priorité | Statut |
|--------|--------------------------------------------------|-----------|---------|
| FA_001 | Création d’un compte utilisateur                 | MUST      | ✅ Fonctionnel |
| FA_002 | Authentification (connexion / déconnexion)       | MUST      | ✅ Fonctionnel |
| FA_003 | Gestion des données personnelles                 | MUST      | ✅ Fonctionnel |
| FA_004 | Traduction de l’interface                        | COULD     | 💡 Évolution future |

---

## ⚡ 2. Profil énergétique et mise en relation

| ID     | Fonctionnalité                                   | Priorité | Statut |
|--------|--------------------------------------------------|-----------|---------|
| FE_001 | Création d’un profil énergétique et point d’accès | MUST    | ✅ Fonctionnel |
| FE_002 | Mise à jour du profil énergétique                | MUST      | ✅ Fonctionnel |
| FE_003 | Découverte de profils compatibles (matching)     | MUST      | ✅ Fonctionnel |
| FE_004 | Géolocalisation des adresses                     | MUST   | ✅ Fonctionnel |
| FE_005 | Simulateur simplifié des gains énergétiques      | SHOULD    | 🔄 Itération future |
| FE_006 | Import automatique des données énergétiques      | COULD     | 💡 Évolution future |
| FE_007 | Simulateur avancé basé sur le facilitateur officiel | COULD  | 💡 Évolution future |
| FE_008 | Gestion des préférences horaires                 | COULD     | 💡 Évolution future |

---

## 🏘️ 3. Gestion du partage d’énergie

| ID     | Fonctionnalité                                   | Priorité | Statut |
|--------|--------------------------------------------------|-----------|---------|
| FP_001 | Créer et gérer un partage                        | MUST      | ✅ Fonctionnel |
| FP_002 | Ajouter ou retirer un membre                     | MUST      | ⚠️ Partiellement implémenté |
| FP_003 | Envoyer une demande au GRD                       | MUST      | ✅ Fonctionnel |
| FP_004 | Validation d’un partage par le GRD              | MUST      | ⚠️ Partiellement implémenté |
| FP_005 | Réponse aux demandes d’information               | MUST      | ✅ Fonctionnel |
| FP_006 | Gestion des documents du partage                 | MUST      | ⏳ Prévu |
| FP_007 | Quitter un partage                               | MUST      | ⏳ Prévu |
| FP_008 | Désactiver un partage                            | MUST      | ⏳ Prévu |
| FP_009 | Télécharger des modèles de documents             | SHOULD    | 🔄 Itération future |
| FP_010 | Suppression automatique des données              | SHOULD    | 🔄 Itération future |
| FP_011 | Partage type “Communauté d’énergie”              | SHOULD    | 🔄 Itération future |
| FP_012 | Signature électronique des documents             | COULD     | 💡 Évolution future |
| FP_013 | Import automatique des données du partage        | COULD     | 💡 Évolution future |
| FP_014 | Génération simplifiée de documents               | COULD     | 💡 Évolution future |
| FP_015 | Gestion intégrée des paiements                   | WON’T     | 🚫 Hors MVP |

---

## 💬 4. Communication et notifications

| ID     | Fonctionnalité                                   | Priorité | Statut |
|--------|--------------------------------------------------|-----------|---------|
| FC_001 | Envoyer un message                               | MUST      | ✅ Fonctionnel |
| FC_002 | Notification de consultation des messages        | MUST      | ✅ Fonctionnel |
| FC_003 | Informations et ressources pédagogiques          | MUST      | ✅ Fonctionnel |
| FC_004 | Notification validation/refus GRD                | MUST      | ⚠️ Partiellement implémenté |
| FC_005 | Notification automatique des matchs              | SHOULD    | 🔄 Itération future |

---

## 📊 5. Tableau de bord et suivi

| ID     | Fonctionnalité                                   | Priorité | Statut |
|--------|--------------------------------------------------|-----------|---------|
| FD_001 | Consultation des données du partage              | MUST      | ⏳ Prévu |
| FD_002 | Consultation d’indicateurs globaux               | SHOULD    | 🔄 Itération future |
| FD_003 | Suivi des demandes de validation                 | MUST      | ⚠️ Partiellement implémenté |
| FD_004 | État des utilisateurs et partages                | SHOULD    | 🔄 Itération future |
| FD_005 | Export des données du partage                    | COULD     | 💡 Évolution future |
| FD_006 | Export consolidé pour organismes publics         | COULD     | 💡 Évolution future |

---

# 📌 Légende

| Symbole | Signification |
|----------|------------------------------|
| ✅ | Fonctionnel |
| ⚠️ | Partiellement implémenté |
| ⏳ | Prévu |
| 🔄 | Itération future |
| 💡 | Évolution future |
| 🚫 | Hors périmètre MVP |
