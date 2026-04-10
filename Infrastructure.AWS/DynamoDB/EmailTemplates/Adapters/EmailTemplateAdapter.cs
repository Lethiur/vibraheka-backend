using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CSharpFunctionalExtensions;
using Infrastructure.AWS.DynamoDB.EmailTemplates.Mappers;
using Infrastructure.AWS.DynamoDB.EmailTemplates.Models;
using Infrastructure.AWS.DynamoDB.Errors;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VibraHeka.Domain.EmailTemplates.Ports.Out;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;

namespace Infrastructure.AWS.DynamoDB.EmailTemplates.Adapters;

/// <summary>
/// Repository for managing email templates stored in a DynamoDB table.
/// Provides functionality to retrieve and save email templates as well as error handling mechanisms.
/// </summary>
/// <remarks>
/// This repository interacts with DynamoDB via AWS SDK and is designed to work with the EmailTemplateDBModel.
/// Implements the IEmailTemplatesRepository interface for application-specific email template operations.
/// Inherits from GenericDynamoRepository for shared data access behaviors.
/// </remarks>
public class EmailTemplateAdapter(
    IDynamoDBContext context,
    IAmazonDynamoDB client,
    IOptionsMonitor<AWSConfig> config,
    EmailTemplateMapper mapper,
    ILogger<EmailTemplateAdapter> logger)
    : GenericDynamoRepository<EmailTemplateDBModel>(context, client, config.CurrentValue.EmailTemplatesTable, logger),
        EmailTemplatePort
{
    /// <summary>
    /// Retrieves an email template entity by its unique identifier.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation. The task result contains a <c>Result</c> object which is successful if the template exists, returning the corresponding <c>EmailEntity</c>; otherwise, it contains an error.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <c>templateID</c> is null or empty.</exception>
    public Task<Result<EmailTemplateEntity>> GetTemplateByID(string templateID, CancellationToken token)
    {
        return FindByID(templateID, token)
            .Ensure(model => model != null, EmailTemplateErrors.TemplateNotFound)
            .Map(mapper.ToDomain)
            .MapError(error =>
            {
                return error switch
                {
                    GenericPersistenceErrors.NoRecordsFound => EmailTemplateErrors.TemplateNotFound,
                    _ => error
                };
            });
    }

    /// <summary>
    /// Retrieves all email templates from the repository and maps them to domain entities.
    /// </summary>
    /// <returns>A <c>Task</c> representing the asynchronous operation.
    /// The task result contains a <c>Result</c> object which encapsulates a collection of <c>EmailEntity</c> instances.</returns>
    /// <exception cref="Exception">Thrown if an error occurs while retrieving or mapping the templates.</exception>
    public Task<Result<IEnumerable<EmailTemplateEntity>>> GetAllTemplates(CancellationToken cancellationToken)
    {
        return GetAll(cancellationToken).Map(list => list.Select(mapper.ToDomain));
    }

    public Task<Result<string>> SaveEmailTemplate(EmailTemplateEntity emailTemplate, CancellationToken token)
    {
        return Save(mapper.FromDomain(emailTemplate), token).Map(_ => emailTemplate.TemplateID);
    }

    public Task<Result<Unit>> EditTemplateName(string templateID, string newTemplateName, CancellationToken token)
    {
        Dictionary<string, AttributeValue> key = new Dictionary<string, AttributeValue>()
        {
            { "TemplateID", new AttributeValue { S = templateID } }
        };

        DynamoExpression update = new DynamoExpression()
        {
            Expression = "set #template_name = :template_name",
            AttributeNames = { ["#template_name"] = "Name" },
            AttributeValues = { { ":template_name", new AttributeValue { S = newTemplateName } } }
        };

        return UpdateAsync(key, update, null, token);
    }
}
