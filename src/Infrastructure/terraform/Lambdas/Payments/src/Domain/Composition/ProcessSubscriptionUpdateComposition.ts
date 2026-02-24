import ISubscriptionRepository from "@Domain/Interfaces/ISubscriptionRepository";
import SubscriptionRepositoryImpl from "@Data/Repository/SubscriptionRepositoryImpl";
import {DynamoDBClient} from "@aws-sdk/client-dynamodb";
import ISubscriptionService from "@Domain/Interfaces/ISubscriptionService";
import SubscriptionService from "@Data/Services/SubscriptionService";
import ProcessUpdateSubscriptionUseCaseImpl
    from "@Application/UseCases/ProcessUpdateSubscription/ProcessUpdateSubscriptionUseCaseImpl";


const Repository : ISubscriptionRepository = new SubscriptionRepositoryImpl(new DynamoDBClient());
const Service : ISubscriptionService = new SubscriptionService(Repository);
const UpdateSubscriptionUseCase = new ProcessUpdateSubscriptionUseCaseImpl(Service);

export {UpdateSubscriptionUseCase};