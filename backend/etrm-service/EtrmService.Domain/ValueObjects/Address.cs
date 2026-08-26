namespace EtrmService.Domain.ValueObjects;

public class Address
{
    public string ZipCode { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;

    protected Address() { }

    public Address(string zipCode, string street, string number, string? complement, string neighborhood, string city, string state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
    }
}
