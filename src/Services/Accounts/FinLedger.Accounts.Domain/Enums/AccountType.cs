namespace FinLedger.Accounts.Domain.Enums;

public enum AccountType
{
    BankSettlement = 1,
    CompanySettlement = 2,
    GovernmentSettlement = 3,
    InternalClearing = 4,
    InternalFee = 5,
    Technical = 6
}
