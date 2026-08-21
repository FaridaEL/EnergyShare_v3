using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.Application.Features.Geocoding
{
    /// <summary>
    /// Résultat d'une opération de géocodage.
    /// Les coordonnées sont exprimées en WGS84 (EPSG:4326).
    /// </summary>
    public record GeocodingResult(
        double Latitude,
        double Longitude,
        double MatchScore
    );
}

