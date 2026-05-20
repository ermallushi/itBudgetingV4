namespace ItBudgetingV4.Web.Domain;

public enum BudgetCategory
{
    CapEx = 1,
    OpEx = 2
}

public enum ContractType
{
    Must = 1,
    Optional = 2
}

public enum BudgetVersionStatus
{
    Draft = 1,
    Final = 2
}

public enum FinancialTransactionType
{
    PR = 1,
    PO = 2,
    Commitment = 3,
    Source = 4
}
