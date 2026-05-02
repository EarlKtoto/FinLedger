namespace FinLedger.Accounts.Domain.Enums;

public enum LimitType
{
    SingleTransactionOutgoing = 1,
    SingleTransactionIncoming = 2,
    DailyOutgoing = 3,
    DailyIncoming = 4,
    MonthlyOutgoing = 5,
    MonthlyIncoming = 6
}
