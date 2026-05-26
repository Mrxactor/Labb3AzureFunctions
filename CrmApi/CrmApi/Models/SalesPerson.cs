
// SalesPerson.cs
// Modellen beskriver den ansvariga säljaren som är kopplad till en kund.
namespace CrmApi.Models
{
    public class SalesPerson
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }
}
