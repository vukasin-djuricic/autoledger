namespace AutoLedger.Domain.Entities;

/// <summary>
/// A single posting line within a journal entry. By the rules of double-entry,
/// a line is either a debit or a credit (never both, never neither).
/// </summary>
public class JournalEntryLine
{
    public int Id { get; private set; }
    public int JournalEntryId { get; private set; }

    public int AccountId { get; private set; }
    public Account? Account { get; private set; }

    public decimal DebitAmount { get; private set; }
    public decimal CreditAmount { get; private set; }

    private JournalEntryLine() { } // EF

    public JournalEntryLine(int accountId, decimal debitAmount, decimal creditAmount)
    {
        if (debitAmount < 0 || creditAmount < 0)
            throw new ArgumentException("Amounts cannot be negative.");
        if (debitAmount > 0 && creditAmount > 0)
            throw new ArgumentException("A line cannot be both a debit and a credit.");
        if (debitAmount == 0 && creditAmount == 0)
            throw new ArgumentException("A line must have a debit or a credit amount.");

        AccountId = accountId;
        DebitAmount = debitAmount;
        CreditAmount = creditAmount;
    }
}
