using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace ThinkVoipTool.Models
{
    public class CompanyModel

    {
        public int id { get; set; }
        public string? identifier { get; set; }
        public string? name { get; set; }
        public Status? status { get; set; }
        public string? addressLine1 { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? zip { get; set; }
        public Country? country { get; set; }
        public string? phoneNumber { get; set; }
        public string? faxNumber { get; set; }
        public string? website { get; set; }
        public Territory? territory { get; set; }
        public string? accountNumber { get; set; }
        public DefaultContact? defaultContact { get; set; }
        public DateTime dateAcquired { get; set; }
        public SicCode? sicCode { get; set; }
        public double annualRevenue { get; set; }
        public TimeZoneSetup? timeZoneSetup { get; set; }
        public bool leadFlag { get; set; }
        public bool unsubscribeFlag { get; set; }
        public string? userDefinedField5 { get; set; }
        public TaxCode? taxCode { get; set; }
        public BillingTerms? billingTerms { get; set; }
        public BillToCompany? billToCompany { get; set; }
        public BillingSite? billingSite { get; set; }
        public BillingContact? billingContact { get; set; }
        public InvoiceDeliveryMethod? invoiceDeliveryMethod { get; set; }
        public string? invoiceToEmailAddress { get; set; }
        public bool deletedFlag { get; set; }
        public string? mobileGuid { get; set; }
        public Currency? currency { get; set; }
        public List<Type>? types { get; set; }
        public Site? site { get; set; }
        public Info13? _info { get; set; }


        public class Info
        {
            public string? status_href { get; set; }
        }

        public class Status
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info? _info { get; set; }
        }

        public class Info2
        {
            public string? country_href { get; set; }
        }

        public class Country
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info2? _info { get; set; }
        }

        public class Info3
        {
            public string? location_href { get; set; }
        }

        public class Territory
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info3? _info { get; set; }
        }

        public class Info4
        {
            public string? contact_href { get; set; }
        }

        public class DefaultContact
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info4? _info { get; set; }
        }

        public class SicCode
        {
            public int id { get; set; }
            public string? name { get; set; }
        }

        public class Info5
        {
            public string? timeZoneSetup_href { get; set; }
        }

        public class TimeZoneSetup
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info5? _info { get; set; }
        }

        public class Info6
        {
            public string? taxCode_href { get; set; }
        }

        public class TaxCode
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info6? _info { get; set; }
        }

        public class BillingTerms
        {
            public int id { get; set; }
            public string? name { get; set; }
        }

        public class Info7
        {
            public string? company_href { get; set; }
        }

        public class BillToCompany
        {
            public int id { get; set; }
            public string? identifier { get; set; }
            public string? name { get; set; }
            public Info7? _info { get; set; }
        }

        public class Info8
        {
            public string? site_href { get; set; }
        }

        public class BillingSite
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info8? _info { get; set; }
        }

        public class Info9
        {
            public string? contact_href { get; set; }
        }

        public class BillingContact
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info9? _info { get; set; }
        }

        public class InvoiceDeliveryMethod
        {
            public int id { get; set; }
            public string? name { get; set; }
        }

        public class Info10
        {
            public string? currency_href { get; set; }
        }

        public class Currency
        {
            public int id { get; set; }
            public string? symbol { get; set; }
            public string? currencyCode { get; set; }
            public string? decimalSeparator { get; set; }
            public int numberOfDecimals { get; set; }
            public string? thousandsSeparator { get; set; }
            public bool negativeParenthesesFlag { get; set; }
            public bool displaySymbolFlag { get; set; }
            public string? currencyIdentifier { get; set; }
            public bool displayIdFlag { get; set; }
            public bool rightAlign { get; set; }
            public string? name { get; set; }
            public Info10? _info { get; set; }
        }

        public class Info11
        {
            public string? type_href { get; set; }
        }

        public class Type
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info11? _info { get; set; }
        }

        public class Info12
        {
            public string? site_href { get; set; }
        }

        public class Site
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Info12? _info { get; set; }
        }

        public class Info13
        {
            public DateTime lastUpdated { get; set; }
            public string? updatedBy { get; set; }
            public DateTime dateEntered { get; set; }
            public string? enteredBy { get; set; }
            public string? contacts_href { get; set; }
            public string? agreements_href { get; set; }
            public string? tickets_href { get; set; }
            public string? opportunities_href { get; set; }
            public string? activities_href { get; set; }
            public string? projects_href { get; set; }
            public string? configurations_href { get; set; }
            public string? orders_href { get; set; }
            public string? documents_href { get; set; }
            public string? sites_href { get; set; }
            public string? teams_href { get; set; }
            public string? reports_href { get; set; }
            public string? notes_href { get; set; }
        }

        public class Agreement
        {
            public int id { get; set; }
            public string? name { get; set; }
            public Type? type { get; set; }
            public BaseModels.Company? company { get; set; }
            public BaseModels.Contact? contact { get; set; }
            public Site? site { get; set; }
            public BaseModels.Location? location { get; set; }
            public BaseModels.Department? department { get; set; }
            public bool restrictLocationFlag { get; set; }
            public bool restrictDepartmentFlag { get; set; }
            public DateTime startDate { get; set; }
            public bool noEndingDateFlag { get; set; }
            public bool cancelledFlag { get; set; }
            public BaseModels.Sla? sla { get; set; }
            public string? applicationUnits { get; set; }
            public double applicationLimit { get; set; }
            public string? applicationCycle { get; set; }
            public bool applicationUnlimitedFlag { get; set; }
            public bool oneTimeFlag { get; set; }
            public bool coverAgreementTime { get; set; }
            public bool coverAgreementProduct { get; set; }
            public bool coverAgreementExpense { get; set; }
            public bool coverSalesTax { get; set; }
            public bool carryOverUnused { get; set; }
            public bool allowOverruns { get; set; }
            public bool expireWhenZero { get; set; }
            public bool chargeToFirm { get; set; }
            public string? employeeCompRate { get; set; }
            public string? employeeCompNotExceed { get; set; }
            public double compHourlyRate { get; set; }
            public double compLimitAmount { get; set; }
            public bool billOneTimeFlag { get; set; }
            public string? invoicingCycle { get; set; }
            public BillToCompany? billToCompany { get; set; }
            public double billAmount { get; set; }
            public bool taxable { get; set; }
            public DateTime billStartDate { get; set; }
            public TaxCode? taxCode { get; set; }
            public bool restrictDownPayment { get; set; }
            public bool prorateFlag { get; set; }
            public bool topComment { get; set; }
            public bool bottomComment { get; set; }
            public string? billTime { get; set; }
            public string? billExpenses { get; set; }
            public string? billProducts { get; set; }
            public bool billableTimeInvoice { get; set; }
            public bool billableExpenseInvoice { get; set; }
            public bool billableProductInvoice { get; set; }
            public Currency? currency { get; set; }
            public bool autoInvoiceFlag { get; set; }
            public string? agreementStatus { get; set; }
            public BaseModels.Info15? _info { get; set; }
        }
    }
}