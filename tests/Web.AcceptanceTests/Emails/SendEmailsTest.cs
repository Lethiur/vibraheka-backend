// using CSharpFunctionalExtensions;
// using NUnit.Framework;
// using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
// using VibraHeka.Domain.Common.Interfaces.User;
// using VibraHeka.Domain.Entities;
// using VibraHeka.Infrastructure.Emails;
// using VibraHeka.Infrastructure.Persistence.Repository;
// using VibraHeka.Infrastructure.Persistence.S3;
// using VibraHeka.Web.AcceptanceTests.Generic;

// namespace VibraHeka.Web.AcceptanceTests.Emails;

// [TestFixture]
// public class SendEmailsTest : GenericAcceptanceTest<VibraHekaProgram>
// {
//     [Test]
//     public async Task ShouldSendEmails()
//     {
//         IUserRepository repository = GetObjectFromFactory<IUserRepository>();
//         EmailClient emailClient = GetObjectFromFactory<EmailClient>();
//         IEmailTemplateStorageRepository templateRepository = GetObjectFromFactory<IEmailTemplateStorageRepository>();

//         Result<string> templateContent = await templateRepository.GetTemplateContent("17876e39-c387-4070-9488-9b591a368f6a", CancellationToken.None);

//         if (templateContent.IsSuccess)
//         {
//             string content = templateContent.Value;

//             Result<IEnumerable<UserEntity>> users = await repository.GetAllAsync(CancellationToken.None);

//             if (users.IsSuccess)
//             {
//                 foreach (UserEntity user in users.Value)
//                 {

//                     string renderedTemplate = content.Replace("{{username}}", user.FirstName);
//                     Result<string> sendEmailAsync = await emailClient.SendEmailAsync($"Comunidad Vibraheka <heka@vibraheka.com>",user.Email, "La oferta terminara pronto", renderedTemplate, "VibraHeka-ses-config-main");
//                     if (sendEmailAsync.IsSuccess)
//                     {
//                         Console.WriteLine(sendEmailAsync.Value);
//                     }
//                 }
//             }

//         }



//     }
// }
