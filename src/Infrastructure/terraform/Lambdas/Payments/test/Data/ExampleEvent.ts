export const ExampleEvent = {
    "version": "0",
    "id": "dd63deb4-3939-774e-9fdf-5cf9457742d0",
    "detail-type": "invoice.paid",
    "source": "aws.partner/stripe.com/ed_test_61U8OhWNACpdhq6GG16U8LJp90CQBOsqL618O7RCiVVA",
    "account": "744711930137",
    "time": "2026-02-16T19:30:44Z",
    "region": "eu-west-1",
    "resources": [
        "arn:aws:events:eu-west-1::event-source/aws.partner/stripe.com/ed_test_61U8OhWNACpdhq6GG16U8LJp90CQBOsqL618O7RCiVVA"
    ],
    "detail": {
        "id": "evt_1T1XVI8p2opQAPTXKPvqkUDU",
        "object": "event",
        "api_version": "2022-11-15",
        "created": 1771270242,
        "data": {
            "object": {
                "id": "in_1T1XVD8p2opQAPTX2UPmI1Sl",
                "object": "invoice",
                "account_country": "ES",
                "account_name": "VibraHeka",
                "account_tax_ids": null,
                "amount_due": 2000,
                "amount_overpaid": 0,
                "amount_paid": 2000,
                "amount_remaining": 0,
                "amount_shipping": 0,
                "application": null,
                "application_fee_amount": null,
                "attempt_count": 0,
                "attempted": true,
                "auto_advance": false,
                "automatic_tax": {
                    "disabled_reason": null,
                    "enabled": false,
                    "liability": null,
                    "provider": null,
                    "status": null
                },
                "automatically_finalizes_at": null,
                "billing_reason": "subscription_create",
                "charge": "ch_3T1XVE8p2opQAPTX1FqphSVi",
                "collection_method": "charge_automatically",
                "created": 1771270239,
                "currency": "eur",
                "custom_fields": null,
                "customer": "cus_TzWXHppRzW1pJG",
                "customer_account": null,
                "customer_address": null,
                "customer_email": "mtesqtsdlc2@gmail.com",
                "customer_name": "Senior pelotas gor das",
                "customer_phone": null,
                "customer_shipping": null,
                "customer_tax_exempt": "none",
                "customer_tax_ids": [],
                "default_payment_method": null,
                "default_source": null,
                "default_tax_rates": [],
                "description": null,
                "discount": null,
                "discounts": [],
                "due_date": null,
                "effective_at": 1771270239,
                "ending_balance": 0,
                "footer": null,
                "from_invoice": null,
                "hosted_invoice_url": "https://invoice.stripe.com/i/acct_1SyxMT8p2opQAPTX/test_YWNjdF8xU3l4TVQ4cDJvcFFBUFRYLF9UeldZdWxZaXFyQXFPRjl2VkFjSGQxV1JlOE5UVE83LDE2MTgxMTA0NA0200gIpjh3h9?s=ap",
                "invoice_pdf": "https://pay.stripe.com/invoice/acct_1SyxMT8p2opQAPTX/test_YWNjdF8xU3l4TVQ4cDJvcFFBUFRYLF9UeldZdWxZaXFyQXFPRjl2VkFjSGQxV1JlOE5UVE83LDE2MTgxMTA0NA0200gIpjh3h9/pdf?s=ap",
                "issuer": {
                    "type": "self"
                },
                "last_finalization_error": null,
                "latest_revision": null,
                "lines": {
                    "object": "list",
                    "data": [
                        {
                            "id": "il_1T1XVD8p2opQAPTXcJPurHlR",
                            "object": "line_item",
                            "amount": 2000,
                            "amount_excluding_tax": 2000,
                            "currency": "eur",
                            "description": "1 × Suscripcion a vibraheka (at €20.00 / month)",
                            "discount_amounts": [],
                            "discountable": true,
                            "discounts": [],
                            "invoice": "in_1T1XVD8p2opQAPTX2UPmI1Sl",
                            "livemode": false,
                            "metadata": {},
                            "parent": {
                                "invoice_item_details": null,
                                "subscription_item_details": {
                                    "invoice_item": null,
                                    "proration": false,
                                    "proration_details": {
                                        "credited_items": null
                                    },
                                    "subscription": "sub_1T1XVG8p2opQAPTXfjGroWVF",
                                    "subscription_item": "si_TzWY9W381nVCZF"
                                },
                                "type": "subscription_item_details"
                            },
                            "period": {
                                "end": 1773689439,
                                "start": 1771270239
                            },
                            "plan": {
                                "id": "price_1SyxN98p2opQAPTX1XXPGLIA",
                                "object": "plan",
                                "active": true,
                                "aggregate_usage": null,
                                "amount": 2000,
                                "amount_decimal": "2000",
                                "billing_scheme": "per_unit",
                                "created": 1770654699,
                                "currency": "eur",
                                "interval": "month",
                                "interval_count": 1,
                                "livemode": false,
                                "metadata": {},
                                "meter": null,
                                "nickname": null,
                                "product": "prod_Twr5q0a7LZq58D",
                                "tiers_mode": null,
                                "transform_usage": null,
                                "trial_period_days": null,
                                "usage_type": "licensed"
                            },
                            "pretax_credit_amounts": [],
                            "price": {
                                "id": "price_1SyxN98p2opQAPTX1XXPGLIA",
                                "object": "price",
                                "active": true,
                                "billing_scheme": "per_unit",
                                "created": 1770654699,
                                "currency": "eur",
                                "custom_unit_amount": null,
                                "livemode": false,
                                "lookup_key": null,
                                "metadata": {},
                                "nickname": null,
                                "product": "prod_Twr5q0a7LZq58D",
                                "recurring": {
                                    "aggregate_usage": null,
                                    "interval": "month",
                                    "interval_count": 1,
                                    "meter": null,
                                    "trial_period_days": null,
                                    "usage_type": "licensed"
                                },
                                "tax_behavior": "inclusive",
                                "tiers_mode": null,
                                "transform_quantity": null,
                                "type": "recurring",
                                "unit_amount": 2000,
                                "unit_amount_decimal": "2000"
                            },
                            "pricing": {
                                "price_details": {
                                    "price": "price_1SyxN98p2opQAPTX1XXPGLIA",
                                    "product": "prod_Twr5q0a7LZq58D"
                                },
                                "type": "price_details",
                                "unit_amount_decimal": "2000"
                            },
                            "proration": false,
                            "proration_details": {
                                "credited_items": null
                            },
                            "quantity": 1,
                            "subscription": "sub_1T1XVG8p2opQAPTXfjGroWVF",
                            "subscription_item": "si_TzWY9W381nVCZF",
                            "subtotal": 2000,
                            "tax_amounts": [],
                            "tax_rates": [],
                            "taxes": [],
                            "type": "subscription",
                            "unit_amount_excluding_tax": "2000"
                        }
                    ],
                    "has_more": false,
                    "total_count": 1,
                    "url": "/v1/invoices/in_1T1XVD8p2opQAPTX2UPmI1Sl/lines"
                },
                "livemode": false,
                "metadata": {},
                "next_payment_attempt": null,
                "number": "583FF902-0012",
                "on_behalf_of": null,
                "paid": true,
                "paid_out_of_band": false,
                "parent": {
                    "quote_details": null,
                    "subscription_details": {
                        "metadata": {},
                        "subscription": "sub_1T1XVG8p2opQAPTXfjGroWVF"
                    },
                    "type": "subscription_details"
                },
                "payment_intent": "pi_3T1XVE8p2opQAPTX1Nmq7EDT",
                "payment_settings": {
                    "default_mandate": null,
                    "payment_method_options": {
                        "acss_debit": null,
                        "bancontact": null,
                        "card": {
                            "request_three_d_secure": "automatic"
                        },
                        "customer_balance": null,
                        "konbini": null,
                        "payto": null,
                        "sepa_debit": null,
                        "us_bank_account": null
                    },
                    "payment_method_types": [
                        "card"
                    ]
                },
                "period_end": 1771270239,
                "period_start": 1771270239,
                "post_payment_credit_notes_amount": 0,
                "pre_payment_credit_notes_amount": 0,
                "quote": null,
                "receipt_number": null,
                "rendering": null,
                "rendering_options": null,
                "shipping_cost": null,
                "shipping_details": null,
                "starting_balance": 0,
                "statement_descriptor": null,
                "status": "paid",
                "status_transitions": {
                    "finalized_at": 1771270239,
                    "marked_uncollectible_at": null,
                    "paid_at": 1771270240,
                    "voided_at": null
                },
                "subscription": "sub_1T1XVG8p2opQAPTXfjGroWVF",
                "subscription_details": {
                    "metadata": {}
                },
                "subtotal": 2000,
                "subtotal_excluding_tax": 2000,
                "tax": null,
                "test_clock": null,
                "total": 2000,
                "total_discount_amounts": [],
                "total_excluding_tax": 2000,
                "total_pretax_credit_amounts": [],
                "total_tax_amounts": [],
                "total_taxes": [],
                "transfer_data": null,
                "webhooks_delivered_at": 1771270239
            }
        },
        "livemode": false,
        "pending_webhooks": 0,
        "request": {
            "id": null,
            "idempotency_key": "fae7870c-7ab6-4490-b8c2-bf4a80f82419"
        },
        "type": "invoice.paid"
    }
}
