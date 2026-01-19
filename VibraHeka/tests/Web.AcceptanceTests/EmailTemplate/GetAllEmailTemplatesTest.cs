using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using NUnit.Framework;
using VibraHeka.Application.Settings.Commands.ChangeTemplateForAction;
using VibraHeka.Domain.Common.Enums;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Models.Results;
using VibraHeka.Web.AcceptanceTests.Generic;

namespace VibraHeka.Web.AcceptanceTests.EmailTemplate;

[TestFixture]
public class GetAllEmailTemplatesTest : GenericAcceptanceTest<VibraHekaProgram>
{
    
    [Test]
    public async Task ShouldUpdateTemplateAndReflectChangeInTemplatesList()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        string newTemplateId = Guid.NewGuid().ToString();
        
        // 1. Insertamos el template en DynamoDB para que la validación del comando pase
        await InsertTemplateInDatabase(newTemplateId);
        
        // 2. Preparamos el usuario Admin y autenticamos
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // 3. Definimos el comando para cambiar el template de verificación
        ChangeTemplateForActionCommand updateCommand = new ChangeTemplateForActionCommand(newTemplateId, ActionType.UserVerification);

        // When
        // 4. Ejecutamos la actualización a través de la API
        HttpResponseMessage updateResponse = await Client.PatchAsJsonAsync("api/v1/settings/ChangeTemplate", updateCommand);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // 5. Consultamos la lista de templates para ver si el cambio se refleja
        HttpResponseMessage getResponse = await Client.GetAsync("api/v1/settings/all-templates");

        // Then
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        ResponseEntity responseEntity = await getResponse.GetAsResponseEntityAndContentAs<IEnumerable<TemplateForActionEntity>>();
        IEnumerable<TemplateForActionEntity>? templates = responseEntity.GetContentAs<IEnumerable<TemplateForActionEntity>>();

        Assert.That(templates, Is.Not.Null);
        Assert.That(templates?.Any(t => t.TemplateID == newTemplateId && t.ActionType == ActionType.UserVerification), Is.True, 
            "The updated template was not found in the settings list.");
    }
    [Test]
    public async Task ShouldReturnOkAndTemplatesListWhenUserIsAdmin()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        
        await RegisterAndConfirmAdmin(username, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        ResponseEntity responseEntity = await response.GetAsResponseEntityAndContentAs<IEnumerable<EmailEntity>>();
        IEnumerable<EmailEntity>? templates = responseEntity.GetContentAs<IEnumerable<EmailEntity>>();
        
        Assert.That(responseEntity.Success, Is.True);
        Assert.That(templates, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenUserIsNotAdmin()
    {
        // Given
        string email = TheFaker.Internet.Email();
        string username = TheFaker.Internet.UserName();
        
        await RegisterAndConfirmUser(username, email, ThePassword);
        AuthenticationResult authResult = await AuthenticateUser(email, ThePassword);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        // When
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenRequestIsUnauthenticated()
    {
        // Given
        // No authentication header is provided

        // When
        HttpResponseMessage response = await Client.GetAsync("api/v1/settings/all-templates");

        // Then
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    private async Task InsertTemplateInDatabase(string templateId)
    {
        // Obtenemos el repositorio real del contenedor de dependencias de la Factory
        IEmailTemplatesRepository repository = GetObjectFromFactory<IEmailTemplatesRepository>();
        
        EmailEntity template = new EmailEntity
        {
            ID = templateId,
            Name = "Acceptance Test Template",
            Path = "test",
            Created = DateTime.UtcNow,
            CreatedBy = "SystemTest"
        };

        await repository.SaveTemplate(template);
    }
    
}
