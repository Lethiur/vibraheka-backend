using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using VibraHeka.Application.Common.Exceptions;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Persistence.DynamoDB.Models;

namespace VibraHeka.Infrastructure.Persistence.Repository;

/// <summary>
/// Repository for managing email templates stored in a DynamoDB table.
/// Provides functionality to retrieve and save email templates as well as error handling mechanisms.
/// </summary>
/// <remarks>
/// This repository interacts with DynamoDB via AWS SDK and is designed to work with the EmailTemplateDBModel.
/// Implements the IEmailTemplatesRepository interface for application-specific email template operations.
/// Inherits from GenericDynamoRepository for shared data access behaviors.
/// </remarks>
public class EmailTemplateRepository(IDynamoDBContext context, IConfiguration config)
    : GenericDynamoRepository<EmailTemplateDBModel>(context, config, "Dynamo:EmailTemplatesTable"), 
        IEmailTemplatesRepository
{
    public async Task<Result<EmailEntity>> GetTemplateByID(string templateID)
    {
        Result<EmailTemplateDBModel> findResult = await FindByID(templateID);
        return findResult
            .Ensure(model => model != null, EmailTemplateErrors.TemplateNotFound)
            .Map(model => model.ToDomain());
    }

    /// <summary>
    /// Saves an email template to the repository.
    /// </summary>
    /// <param name="template">The email template entity to be saved.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation.
    /// The task result contains a <c>Result</c> object indicating the success or failure of the operation.</returns>
    /// <exception cref="NotImplementedException">Thrown if the method is not yet implemented.</exception>
    public async Task<Result<Unit>> SaveTemplate(EmailEntity template)
    {
        return await Save(EmailTemplateDBModel.FromDomain(template));
    }

    /// <summary>
    /// Retrieves all email templates from the repository and maps them to domain entities.
    /// </summary>
    /// <returns>A <c>Task</c> representing the asynchronous operation.
    /// The task result contains a <c>Result</c> object which encapsulates a collection of <c>EmailEntity</c> instances.</returns>
    /// <exception cref="Exception">Thrown if an error occurs while retrieving or mapping the templates.</exception>
    public Task<Result<IEnumerable<EmailEntity>>> GetAllTemplates(CancellationToken cancellationToken)
    {
        return GetAll(cancellationToken).Map(list =>
        {
            return list.Select(model => model.ToDomain());
        });
    }

    /// <summary>
    /// Handles errors encountered during the execution of repository operations by converting exceptions into error messages.
    /// </summary>
    /// <param name="ex">The exception thrown during the operation.</param>
    /// <returns>A string representation of the error message derived from the exception.</returns>
    protected override string HandleError(Exception ex)
    {
        return ex.Message;
    }
}
