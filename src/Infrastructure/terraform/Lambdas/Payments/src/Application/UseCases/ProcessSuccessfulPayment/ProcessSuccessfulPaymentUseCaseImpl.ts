import {SubscriptionErrors} from "@/Domain/Errors/SubscriptionErrors";
import IProcessSuccessfulPaymentUseCase
    from "@Application/UseCases/ProcessSuccessfulPayment/IProcessSuccessfulPaymentUseCase";
import {Result} from "neverthrow";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import Stripe from "stripe";
import INotificationService from "@Domain/Interfaces/INotificationService";
import NotificationEmailEventDetail from "@Domain/Events/NotificationEmailEvent";
import SubscriptionEntity from "@Domain/Entities/SubscriptionEntity";
import {EmailNotificationErrors} from "@Domain/Errors/EmailNotificationErrors";


export default class ProcessSuccessfulPaymentUseCaseImpl implements IProcessSuccessfulPaymentUseCase {

    constructor(private readonly SubscriptionService: ISubscriptionService, private readonly NotificationService: INotificationService) {
    }

    public async Execute(subscriptionData: Stripe.Invoice): Promise<Result<void, SubscriptionErrors>> {
        const result = (await this.SubscriptionService.ProcessPayment(subscriptionData));

        if (result.isOk()) {
            const entity: SubscriptionEntity = result.value;
            console.log("Invoice URL: ", subscriptionData.invoice_pdf!);
            const notificationEvent: NotificationEmailEventDetail = {
                username: subscriptionData.customer_name!,
                attachments: [{
                    attachmentName: "factura.pdf",
                    attachmentUrl: subscriptionData.invoice_pdf!,
                    attachmentType: "application/pdf"
                }],
                recipient: subscriptionData.customer_email!,
                templateType: "subscription_thank_you",
                subject: "Gracias por tu subscripcion",
                templateData: {
                    username: subscriptionData.customer_name!,
                    trialEnd: entity.EndDate,
                    trialEndPeriodNotificationDays: 3
                }
            };
            const newVar: Result<void, EmailNotificationErrors> = await this.NotificationService.Publish(notificationEvent);

            return newVar.map(_ => undefined).mapErr(error => {
                console.error("Problem while sending the email notification to customer", error);
                return SubscriptionErrors.UNEXPECTED_ERROR;
            });
        }

        return result.map(_ => undefined);
    }
}