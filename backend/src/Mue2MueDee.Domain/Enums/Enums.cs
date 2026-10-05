namespace Mue2MueDee.Domain.Enums;

public enum StoreRole
{
    Owner = 1,
    Admin = 2,
    Staff = 3
}

public enum InventoryStatus
{
    Draft = 1,
    PendingInspection = 2,
    Available = 3,
    Reserved = 4,
    Sold = 5,
    Repair = 6,
    Returned = 7,
    Damaged = 8,
    WrittenOff = 9
}

public enum PaymentMethod
{
    Cash = 1,
    BankTransfer = 2,
    PromptPay = 3,
    CreditCard = 4,
    Other = 99
}

public enum PaymentDirection
{
    In = 1,   // Received from Customer (Sales)
    Out = 2   // Paid to Customer (Purchase)
}

public enum CostType
{
    Purchase = 1,
    Repair = 2,
    Cleaning = 3,
    Shipping = 4,
    Other = 99
}

public enum CategoryType
{
    Camera = 1,
    Lens = 2,
    Flash = 3,
    Accessory = 4,
    Other = 99
}

public enum CustomerType
{
    Seller = 1,
    Buyer = 2,
    Both = 3
}

public enum StockMovementType
{
    Receive = 1,
    Reserve = 2,
    CancelReservation = 3,
    Sell = 4,
    Return = 5,
    StatusChange = 6,
    CostAdjustment = 7,
    WriteOff = 8
}

public enum PurchaseStatus
{
    Draft = 1,
    Appraising = 2,
    Offered = 3,
    Accepted = 4,
    Rejected = 5,
    Paid = 6,
    Received = 7,
    Cancelled = 8
}

public enum InspectionItemResult
{
    Pass = 1,
    MinorIssue = 2,
    Fail = 3,
    NotApplicable = 4
}

public enum SaleStatus
{
    Draft = 1,
    Completed = 2,
    Cancelled = 3,
    Refunded = 4
}

