namespace EtrmService.Domain.ValueObjects;

public class ContactInfo
{
    public string? GeneralEmail { get; private set; }
    public string? LegalEmail { get; private set; }
    public string? FinancialEmail { get; private set; }
    public string? Phone1 { get; private set; }
    public string? Phone2 { get; private set; }
    public string? Phone3 { get; private set; }

    protected ContactInfo() { }

    public ContactInfo(string? generalEmail, string? legalEmail, string? financialEmail, string? phone1, string? phone2, string? phone3)
    {
        GeneralEmail = generalEmail;
        LegalEmail = legalEmail;
        FinancialEmail = financialEmail;
        Phone1 = phone1;
        Phone2 = phone2;
        Phone3 = phone3;
    }
}
