using System.ComponentModel.DataAnnotations;

namespace VibraHeka.Infrastructure.Entities;

/// <summary>
/// Represents the configuration required for integrating with the Stripe API.
/// </summary>
public class StripeConfig
{
    /// <summary>
    /// Gets or sets the secret key used to authenticate requests to the Stripe API.
    /// This key is critical for secure operations and should be stored and accessed securely.
    /// </summary>
    [Required]
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to which the user is redirected upon a successful payment.
    /// This URL is typically used to display a confirmation or success message to the user.
    /// It is important to ensure that this URL is correctly configured and accessible to the user.
    /// </summary>
    [Required]
    public string PaymentSuccessUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to which the user is redirected when a payment process is canceled.
    /// This URL serves as a fallback destination in case the transaction does not complete successfully.
    /// </summary>
    [Required]
    public string PaymentCancelUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of payment methods accepted during the Stripe payment process.
    /// These methods define the types of payment options users can choose from,
    /// such as card, ACH, or other supported methods.
    /// </summary>
    [Required]
    public List<string> PaymentMethodsAccepted { get; set; } = [];

    /// <summary>
    /// Gets or sets the subscription ID associated with a customer's subscription in Stripe.
    /// This identifier is used to uniquely reference and manage subscriptions within Stripe's API.
    /// </summary>
    [Required]
    public string SubscriptionID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of days for the trial period associated with a subscription.
    /// This value determines the duration during which a user can use the service without incurring charges.
    /// </summary>
    public int TrialPeriodInDays { get; set; } = 0;
}
