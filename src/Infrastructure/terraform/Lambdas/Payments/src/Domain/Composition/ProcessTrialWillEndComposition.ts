import NotificationService from "@Data/Services/NotificationService";
import TrialWillEndUseCaseImpl from "@Application/UseCases/ProcessTrialWillEnd/TrialWillEndUseCaseImpl";
import ITrialWillEndUseCase from "@Application/UseCases/ProcessTrialWillEnd/ITrialWillEndUseCase";


export const ProcessTrialWillEndUseCase : ITrialWillEndUseCase = new TrialWillEndUseCaseImpl(new NotificationService());