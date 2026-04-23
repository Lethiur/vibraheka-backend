using System.Net.Http.Json;
using VibraHeka.Domain.Recordings.Enums;
using VibraHeka.Web.AcceptanceTests.Generic;
using VibraHeka.Web.Entities;

namespace VibraHeka.Web.AcceptanceTests.Recordings;

/// <summary>
/// Helpers para construir cuerpos de petición JSON de grabaciones.
/// Para autenticación y registro usar los métodos heredados de <see cref="Generic.GenericAcceptanceTest{TAppClass}"/>.
/// </summary>
public abstract class GenericRecordingsTest : GenericAcceptanceTest<VibraHekaProgram>
{
    protected UploadRecordingRequest BuildValidBody() => BuildBody();

    protected UploadRecordingRequest BuildBody(
        string name = "Sesion de meditacion",
        string description = "Descripcion valida de la sesion de meditacion guiada",
        RecordingType type = RecordingType.Meditacion)
    {
        UploadRecordingRequest request = new()
        {
            Name = name,
            Description = description,
            Type = type
        };

        return request;
    }
}
