
// Customer.cs
// Modellen beskriver en kund i CRM-systemet och innehåller även ansvarig säljare.
namespace CrmApi.Models
{
    public class Customer
    {
        public string id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email {  get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public SalesPerson SalesPerson { get; set; } = new();
    }   
}
