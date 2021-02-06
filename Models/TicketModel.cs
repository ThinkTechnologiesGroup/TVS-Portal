using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace ThinkVoipTool.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 


    public class TicketModel : BaseModels
    {
        public int id { get; set; }
        public string? summary { get; set; }
        public string? recordType { get; set; }
        public Board? board { get; set; }
        public Status? status { get; set; }
        public Company? company { get; set; }
        public string? addressLine1 { get; set; }
        public string? city { get; set; }
        public string? zip { get; set; }
        public Country? country { get; set; }
        public Contact? contact { get; set; }
        public string? contactName { get; set; }
        public string? contactPhoneNumber { get; set; }
        public string? contactEmailAddress { get; set; }
        public Team? team { get; set; }
        public Priority? priority { get; set; }
        public DateTime requiredDate { get; set; }
        public string? severity { get; set; }
        public string? impact { get; set; }
        public bool allowAllClientsPortalView { get; set; }
        public bool customerUpdatedFlag { get; set; }
        public bool automaticEmailContactFlag { get; set; }
        public bool automaticEmailResourceFlag { get; set; }
        public bool automaticEmailCcFlag { get; set; }
        public DateTime closedDate { get; set; }
        public string? closedBy { get; set; }
        public bool closedFlag { get; set; }
        public bool approved { get; set; }
        public double estimatedExpenseCost { get; set; }
        public double estimatedExpenseRevenue { get; set; }
        public double estimatedProductCost { get; set; }
        public double estimatedProductRevenue { get; set; }
        public double estimatedTimeCost { get; set; }
        public double estimatedTimeRevenue { get; set; }
        public string? billingMethod { get; set; }
        public string? subBillingMethod { get; set; }
        public DateTime dateResolved { get; set; }
        public DateTime dateResplan { get; set; }
        public DateTime dateResponded { get; set; }
        public int resolveMinutes { get; set; }
        public int resPlanMinutes { get; set; }
        public int respondMinutes { get; set; }
        public bool isInSla { get; set; }
        public string? resources { get; set; }
        public bool hasChildTicket { get; set; }
        public bool hasMergedChildTicketFlag { get; set; }
        public string? billTime { get; set; }
        public string? billExpenses { get; set; }
        public string? billProducts { get; set; }
        public Location? location { get; set; }
        public Department? department { get; set; }
        public string? mobileGuid { get; set; }
        public Sla? sla { get; set; }
        public Currency? currency { get; set; }
        public Info12? _info { get; set; }
        public List<CustomField>? customFields { get; set; }
        public Site? site { get; set; }
        public string? siteName { get; set; }
        public string? addressLine2 { get; set; }
        public string? stateIdentifier { get; set; }
        public Type? type { get; set; }
        public ServiceLocation? serviceLocation { get; set; }
        public Source? source { get; set; }
        public string? externalXRef { get; set; }
        public Owner? owner { get; set; }
        public string? automaticEmailCc { get; set; }
        public Agreement? agreement { get; set; }
    }
}