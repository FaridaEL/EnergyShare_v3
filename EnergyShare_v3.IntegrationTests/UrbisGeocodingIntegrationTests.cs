using EnergyShare_v3.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnergyShare_v3.IntegrationTests
{
    public class UrbisGeocodingIntegrationTests
       : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public UrbisGeocodingIntegrationTests(
            CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Vérifie qu'une adresse réelle située à Bruxelles est correctement géocodée par UrbIS.
        /// </summary>
        [Fact]
        public async Task GeocodeAsync_WithValidBrusselsAddress_ShouldReturnCoordinates()
        {
            using var scope = _factory.Services.CreateScope();

            var geocodingService = scope.ServiceProvider
                .GetRequiredService<IGeocodingService>();

            var result = await geocodingService.GeocodeAsync(
                "Avenue des Arts 21",
                "1000");

            result.Should().NotBeNull();

            result!.Latitude.Should().BeApproximately(50.8461,0.001);

            result.Longitude.Should().BeApproximately(4.3693,0.001);
        }

        /// <summary>
        /// Vérifie qu'une adresse sans numéro ne peut pas être géocodée.
        /// </summary>
        [Fact]
        public async Task GeocodeAsync_WithoutStreetNumber_ShouldReturnNull()
        {
            using var scope = _factory.Services.CreateScope();

            var geocodingService = scope.ServiceProvider
                .GetRequiredService<IGeocodingService>();

            var result = await geocodingService.GeocodeAsync("Avenue des Arts","1000");

            result.Should().BeNull();
        }

        /// <summary>
        /// Vérifie qu'une adresse inexistante ne retourne pas de coordonnées exploitables.
        /// </summary>
        //[Fact]
        //public async Task GeocodeAsync_WithUnknownAddress_ShouldReturnNull()
        //{
        //    using var scope = _factory.Services.CreateScope();

        //    var geocodingService = scope.ServiceProvider
        //        .GetRequiredService<IGeocodingService>();

        //    var result = await geocodingService.GeocodeAsync("Rue Totalement Inexistante 9999", "1000");

        //    result.Should().BeNull();
        //}


       
        /// <summary>
        /// Vérifie qu'une adresse située hors de Bruxelles n'est pas reconnue comme adresse valide du périmètre UrbIS.
        /// </summary>
        [Fact]
        public async Task GeocodeAsync_WithAddressOutsideBrussels_ShouldReturnNull()
        {
            using var scope = _factory.Services.CreateScope();

            var geocodingService = scope.ServiceProvider
                .GetRequiredService<IGeocodingService>();

            var result = await geocodingService.GeocodeAsync( "Meir 1", "2000");

            result.Should().BeNull();
        }
        /// <summary>
        /// Vérifie qu'une recherche partielle d'adresse retourne des suggestions d'adresses bruxelloises via UrbIS.
        /// </summary>
        [Fact]
        public async Task SearchAddressesAsync_WithPartialStreet_ShouldReturnSuggestions()
        {
            using var scope = _factory.Services.CreateScope();

            var geocodingService = scope.ServiceProvider
                .GetRequiredService<IGeocodingService>();

            var results = await geocodingService.SearchAddressesAsync(
                street: "avenue des ar",
                number: "21",
                postalCode: "1000");

            results.Should().NotBeEmpty();

            results.Should().Contain(x =>
                x.Street == "Avenue des Arts" &&
                x.Number == "21" &&
                x.PostalCode == "1000");
        }

        /// <summary>
        /// Vérifie qu'une saisie trop courte ne déclenche pas de recherche d'adresse inutile.
        /// </summary>
        [Fact]
        public async Task SearchAddressesAsync_WithTooShortStreet_ShouldReturnEmptyList()
        {
            using var scope = _factory.Services.CreateScope();

            var geocodingService = scope.ServiceProvider
                .GetRequiredService<IGeocodingService>();

            var results = await geocodingService.SearchAddressesAsync(
                street: "av",
                number: null,
                postalCode: null);

            results.Should().BeEmpty();
        }



    }
}
