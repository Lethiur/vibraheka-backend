import Stripe from "stripe";

export default interface IProcessFailedPayment {
    Execute(invoicePayed : Stripe.Invoice): Promise<void>;
}