using System;

namespace FleetManagementSystem.Data.Entities
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyNameEn { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string CommercialRegistration { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = "EGP";
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string TimeFormat { get; set; } = "HH:mm";
        public int DecimalPlaces { get; set; } = 2;
        public string DefaultLanguage { get; set; } = "ar";
        public string DefaultTheme { get; set; } = "Light";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
