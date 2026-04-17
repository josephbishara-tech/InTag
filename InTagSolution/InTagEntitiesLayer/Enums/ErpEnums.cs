namespace InTagEntitiesLayer.Enums
{
    // ── Sales ──
    public enum QuotationStatus
    {
        Draft = 0,
        Sent = 1,
        Confirmed = 2,
        Expired = 3,
        Cancelled = 4
    }

    public enum SalesOrderStatus
    {
        Draft = 0,
        Confirmed = 1,
        InDelivery = 2,
        Delivered = 3,
        Invoiced = 4,
        Done = 5,
        Cancelled = 6
    }

    // ── Purchase ──
    public enum RfqStatus
    {
        Draft = 0,
        Sent = 1,
        Received = 2,
        Selected = 3,
        Cancelled = 4
    }

    public enum PurchaseOrderStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Approved = 2,
        Confirmed = 3,
        PartialReceipt = 4,
        Received = 5,
        Billed = 6,
        Done = 7,
        Cancelled = 8
    }

    // ── Accounting ──
    public enum InvoiceType
    {
        CustomerInvoice = 0,
        VendorBill = 1,
        CustomerCreditNote = 2,
        VendorDebitNote = 3
    }

    public enum InvoiceStatus
    {
        Draft = 0,
        Posted = 1,
        PartiallyPaid = 2,
        Paid = 3,
        Cancelled = 4
    }

    public enum PaymentMethod
    {
        Cash = 0,
        BankTransfer = 1,
        Check = 2,
        CreditCard = 3,
        Other = 4
    }

    public enum JournalType
    {
        Sales = 0,
        Purchase = 1,
        Bank = 2,
        Cash = 3,
        Miscellaneous = 4
    }

    public enum JournalEntryState
    {
        Draft = 0,
        Posted = 1
    }

    public enum AccountType
    {
        Asset = 0,
        Liability = 1,
        Equity = 2,
        Revenue = 3,
        Expense = 4,
        CostOfGoodsSold = 5
    }

    // ── Stock Move ──
    public enum StockMoveStatus
    {
        Draft = 0,
        Confirmed = 1,
        Done = 2,
        Cancelled = 3
    }

    public enum StockMoveType
    {
        Receipt = 0,
        Delivery = 1,
        Internal = 2,
        Adjustment = 3,
        Scrap = 4,
        Production = 5
    }

    public enum LocationType
    {
        Internal = 0,
        Input = 1,
        Output = 2,
        Virtual = 3,
        Scrap = 4
    }
}
