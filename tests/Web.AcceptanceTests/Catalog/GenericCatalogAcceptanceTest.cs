using NMoneys;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.Catalog;

/// <summary>
/// Base class for Catalog acceptance tests.
/// Provides shared endpoint constant and request builders.
/// Authentication helpers are inherited from <see cref="GenericAcceptanceTest{TAppClass}"/>.
/// </summary>
public abstract class GenericCatalogAcceptanceTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected const string CatalogEndpoint = "/api/v1/catalog";

    protected static CreateProductRequest BuildValidRequest() =>
        new CreateProductRequest
        {
            Name = "Meditacion Matutina",
            Description = "Sesion de meditacion guiada para el inicio del dia",
            Price = 9.99m,
            CurrencyCode = CurrencyIsoCode.EUR,
        };
}

