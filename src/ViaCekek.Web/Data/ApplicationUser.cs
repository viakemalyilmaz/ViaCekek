using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ViaCekek.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    // Ad Soyad — giriş UserName ile yapılır, bu sadece görüntüleme/tanıma
    // amaçlı (bkz. CLAUDE.md > Kullanıcı Yönetimi).
    [MaxLength(200)]
    public string? Name { get; set; }
}

