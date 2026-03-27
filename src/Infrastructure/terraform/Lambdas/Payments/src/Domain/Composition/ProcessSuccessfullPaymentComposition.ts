import SubscriptionRepositoryImpl from "@Data/Repository/SubscriptionRepositoryImpl";
import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import {DynamoDBClient} from "@aws-sdk/client-dynamodb";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import ProcessSuccessfulPaymentUseCaseImpl
    from "@Application/UseCases/ProcessSuccessfulPayment/ProcessSuccessfulPaymentUseCaseImpl";
import SubscriptionService from "@Data/Services/SubscriptionService";
import NotificationService from "@Data/Services/NotificationService";

const Repository : ISubscriptionRepository = new SubscriptionRepositoryImpl(new DynamoDBClient());
const Service : ISubscriptionService = new SubscriptionService(Repository);
const UseCase = new ProcessSuccessfulPaymentUseCaseImpl(Service, new NotificationService());

export {UseCase};