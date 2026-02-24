import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import SubscriptionRepositoryImpl from "@Data/Repository/SubscriptionRepositoryImpl";
import {DynamoDBClient} from "@aws-sdk/client-dynamodb";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import SubscriptionService from "@Data/Services/SubscriptionService";
import ProcessCancelSubscriptionUseCaseImpl
    from "@Application/UseCases/ProcessCancelSubscription/ProcessCancelSubscriptionUseCaseImpl";


const Repository : ISubscriptionRepository = new SubscriptionRepositoryImpl(new DynamoDBClient());
const Service : ISubscriptionService = new SubscriptionService(Repository);
const CancelSubscriptionUseCase = new ProcessCancelSubscriptionUseCaseImpl(Service);

export {CancelSubscriptionUseCase};