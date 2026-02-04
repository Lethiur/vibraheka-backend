using Amazon.DynamoDBv2.DataModel;
using CSharpFunctionalExtensions;
using MediatR;
using VibraHeka.Domain.Common.Interfaces.EmailTemplates;
using VibraHeka.Domain.Entities;
using VibraHeka.Domain.Exceptions;
using VibraHeka.Infrastructure.Entities;
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
public class EmailTemplateRepository(IDynamoDBContext context, AWSConfig config)
    : GenericDynamoRepository<EmailTemplateDBModel>(context, config.EmailTemplatesTable), 
        IEmailTemplatesRepository
{
    /// <summary>
    /// Retrieves an email template entity by its unique identifier.
    /// </summary>
    /// <param name="templateID">The unique identifier of the email template to retrieve.</param>
    /// <returns>A <c>Task</c> representing the asynchronous operation. The task result contains a <c>Result</c> object which is successful if the template exists, returning the corresponding <c>EmailEntity</c>; otherwise, it contains an error.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <c>templateID</c> is null or empty.</exception>
    public Task<Result<EmailEntity>> GetTemplateByID(string templateID)
    {
        return FindByID(templateID)
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
    public async Task<Result<Unit>> SaveTemplate(EmailEntity template, CancellationToken token)
    {
        return await Save(EmailTemplateDBModel.FromDomain(template), token);
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
