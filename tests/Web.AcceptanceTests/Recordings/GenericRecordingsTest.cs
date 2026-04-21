using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
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
    private static readonly byte[] DefaultFileBytes = [1, 2, 3, 4, 5];

    protected MultipartFormDataContent BuildValidBody() => BuildBody();

    protected MultipartFormDataContent BuildBodyWithFile(byte[] fileBytes, string fileName) =>
        BuildBody(fileBytes: fileBytes, fileName: fileName);

    protected MultipartFormDataContent BuildBody(
        string name = "Sesion de meditacion",
        string description = "Descripcion valida de la sesion de meditacion guiada",
        RecordingType type = RecordingType.Meditacion,
        byte[]? fileBytes = null,
        string fileName = "meditacion.mp4")
    {
        MultipartFormDataContent form = new();

        form.Add(new StringContent(name, null, "text/plain"), "name");
        form.Add(new StringContent(description, null, "text/plain"), "description");
        form.Add(new StringContent(type.GetHashCode().ToString(), null, "text/plain"), "type");

        MemoryStream fileStream = new(fileBytes ?? DefaultFileBytes);
        StreamContent filePart = new(fileStream);
        filePart.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");
        form.Add(filePart, "file", fileName);

        return form;

    }

}
