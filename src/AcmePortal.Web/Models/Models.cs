namespace AcmePortal.Web.Models;

public record Customer(long Id, string Name, string Email);

public record Order(
    long Id,
    long CustomerId,
    string Description,
    decimal Total,
    string Status,
    DateTime PlacedOn);
