using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace VibraHeka.Infrastructure.Emails;

public class EmailClient(IAmazonSimpleEmailServiceV2 sesClient, ILogger<EmailClient> logger)
{
    public async Task<Result<string>> SendEmailAsync(string from, string destination, string subject, string htmlContent,
        string configSetName)
    {
        SendEmailRequest request = new SendEmailRequest()
        {
            FromEmailAddress = from,
            ConfigurationSetName = configSetName,
            Destination = new Destination() { ToAddresses = [destination] },
            Content = new EmailContent()
            {
                Simple = new Message()
                {
                    Subject = new Content() { Charset = "UTF-8", Data = subject },
                    Body = new Body() { Html = new Content() { Charset = "UTF-8", Data = htmlContent } }
                }
            }
        };

        SendEmailResponse sendEmailResponse = await sesClient.SendEmailAsync(request);
        logger.LogInformation("Email sent successfully with messageID {MessageId} with subject {Subject}", sendEmailResponse.MessageId, subject);
        return Result.Success(sendEmailResponse.MessageId);
    }
}
