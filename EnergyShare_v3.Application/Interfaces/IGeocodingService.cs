using EnergyShare_v3.Application.Features.Geocoding;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Interfaces
{
    /// <summary>
    /// Contrat permettant de géocoder une adresse sans exposer
    /// l'implémentation concrète utilisée par l'Infrastructure.
    /// </summary>
    public interface IGeocodingService
    {
        Task<GeocodingResult?> GeocodeAsync(
            string adresseLine1,
            string codePostal,
            CancellationToken cancellationToken = default);
    }
}
