using System;

namespace ThinkVoip.Models
{
    public class BaseModels
    {
        public class Info
        {
            public string status_href { get; set; }
        }

        public class Status
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info _info { get; set; }
        }

        public class Info2
        {
            public string country_href { get; set; }
        }

        public class Country
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info2 _info { get; set; }
        }

        public class Info3
        {
            public string location_href { get; set; }
        }

        public class Territory
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info3 _info { get; set; }
        }

        public class Info4
        {
            public string contact_href { get; set; }
        }

        public class DefaultContact
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info4 _info { get; set; }
        }

        public class SicCode
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Info5
        {
            public string timeZoneSetup_href { get; set; }
        }

        public class TimeZoneSetup
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info5 _info { get; set; }
        }

        public class Info6
        {
            public string taxCode_href { get; set; }
        }

        public class TaxCode
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info6 _info { get; set; }
        }

        public class BillingTerms
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Info7
        {
            public string company_href { get; set; }
        }

        public class BillToCompany
        {
            public int id { get; set; }
            public string identifier { get; set; }
            public string name { get; set; }
            public Info7 _info { get; set; }
        }

        public class Info8
        {
            public string site_href { get; set; }
        }

        public class BillingSite
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info8 _info { get; set; }
        }

        public class Info9
        {
            public string contact_href { get; set; }
        }

        public class BillingContact
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info9 _info { get; set; }
        }

        public class InvoiceDeliveryMethod
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Info10
        {
            public string currency_href { get; set; }
        }

        public class Currency
        {
            public int id { get; set; }
            public string symbol { get; set; }
            public string currencyCode { get; set; }
            public string decimalSeparator { get; set; }
            public int numberOfDecimals { get; set; }
            public string thousandsSeparator { get; set; }
            public bool negativeParenthesesFlag { get; set; }
            public bool displaySymbolFlag { get; set; }
            public string currencyIdentifier { get; set; }
            public bool displayIdFlag { get; set; }
            public bool rightAlign { get; set; }
            public string name { get; set; }
            public Info10 _info { get; set; }
        }

        public class Info11
        {
            public string type_href { get; set; }
        }

        public class Type
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info11 _info { get; set; }
        }

        public class Info12
        {
            public string site_href { get; set; }
        }

        public class Site
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info12 _info { get; set; }
        }

        public class Info13
        {
            public DateTime lastUpdated { get; set; }
            public string updatedBy { get; set; }
            public DateTime dateEntered { get; set; }
            public string enteredBy { get; set; }
            public string contacts_href { get; set; }
            public string agreements_href { get; set; }
            public string tickets_href { get; set; }
            public string opportunities_href { get; set; }
            public string activities_href { get; set; }
            public string projects_href { get; set; }
            public string configurations_href { get; set; }
            public string orders_href { get; set; }
            public string documents_href { get; set; }
            public string sites_href { get; set; }
            public string teams_href { get; set; }
            public string reports_href { get; set; }
            public string notes_href { get; set; }
        }

        public class Board
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info _info { get; set; }
        }


        public class Company
        {
            public int id { get; set; }
            public string identifier { get; set; }
            public string name { get; set; }
            public Info3 _info { get; set; }
        }


        public class Contact
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info5 _info { get; set; }
        }


        public class Team
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info6 _info { get; set; }
        }


        public class Priority
        {
            public int id { get; set; }
            public string name { get; set; }
            public int sort { get; set; }
            public Info7 _info { get; set; }
        }


        public class Location
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info8 _info { get; set; }
        }


        public class Department
        {
            public int id { get; set; }
            public string identifier { get; set; }
            public string name { get; set; }
            public Info9 _info { get; set; }
        }


        public class Sla
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info10 _info { get; set; }
        }


        public class CustomField
        {
            public int id { get; set; }
            public string caption { get; set; }
            public string type { get; set; }
            public string entryMethod { get; set; }
            public int numberOfDecimals { get; set; }
        }


        public class Info14
        {
            public string type_href { get; set; }
        }


        public class Info15
        {
            public string location_href { get; set; }
        }

        public class ServiceLocation
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info15 _info { get; set; }
        }

        public class Info16
        {
            public string source_href { get; set; }
        }

        public class Source
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info16 _info { get; set; }
        }

        public class Info17
        {
            public string member_href { get; set; }
            public string image_href { get; set; }
        }

        public class Owner
        {
            public int id { get; set; }
            public string identifier { get; set; }
            public string name { get; set; }
            public Info17 _info { get; set; }
        }

        public class Info18
        {
            public string agreement_href { get; set; }
        }

        public class Agreement
        {
            public int id { get; set; }
            public string name { get; set; }
            public Info18 _info { get; set; }
        }
    }
}