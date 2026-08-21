using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Geocoding
{
   // Représente une adresse proposée à l'utilisateur lors de la recherche d'une adresse bruxelloise. 
    public record AddressSuggestion(
        string Street,
        string Number,
        string PostalCode,
        string Municipality,
        double Score   //classement de la pertinence de la suggestion (0 à 1, 1 étant le plus pertinent)
    )
    {
        // Texte complet affichable dans l'interface.Ex : "Avenue des Arts 21 - 1000 Bruxelles".  
        public string DisplayText {  
            get { 
                return $"{Street} {Number} - {PostalCode} {Municipality}";
            }
        }
    }
}
