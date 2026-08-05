using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ViaCekek.Web.Models;
using ViaCekek.Web.Models.Common;

namespace ViaCekek.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    AuthenticationStateProvider authenticationStateProvider) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Tekne> Tekneler => Set<Tekne>();
    public DbSet<Kisi> Kisiler => Set<Kisi>();
    public DbSet<Arac> Araclar => Set<Arac>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tekne>()
            .HasIndex(t => t.TekneKodu)
            .IsUnique();

        builder.Entity<Kisi>()
            .HasIndex(k => k.KimlikNumarasi)
            .IsUnique();

        // FirmaAdi'nda autocomplete sorgusu (DISTINCT + arama) hızlı çalışsın diye.
        builder.Entity<Kisi>()
            .HasIndex(k => k.FirmaAdi);

        // Yasaklanma Sebebi yalnızca Pasif kişilerde girilebilir.
        builder.Entity<Kisi>().ToTable(t => t.HasCheckConstraint(
            "CK_Kisiler_YasaklanmaSebebi_Aktif",
            "[Aktif] = 1 AND [YasaklanmaSebebi] IS NULL OR [Aktif] = 0"));

        builder.Entity<Arac>()
            .HasIndex(a => a.TakipNumarasi)
            .IsUnique();

        builder.Entity<Arac>()
            .HasIndex(a => a.FirmaAdi);

        builder.Entity<Arac>()
            .Property(a => a.AracTuru)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Entity<Arac>().ToTable(t => t.HasCheckConstraint(
            "CK_Araclar_YasaklanmaSebebi_Aktif",
            "[Aktif] = 1 AND [YasaklanmaSebebi] IS NULL OR [Aktif] = 0"));
    }

    // Blazor Server interactive circuit'lerde HttpContext güvenilir olmadığından
    // (SignalR bağlantısı kurulduktan sonra ilk isteğe ait HttpContext elden çıkar),
    // audit alanları yalnızca asenkron yolda, AuthenticationStateProvider üzerinden
    // doldurulur. Senkron SaveChanges bilerek desteklenmiyor.
    public override int SaveChanges()
        => throw new InvalidOperationException(
            "SaveChanges yerine SaveChangesAsync kullanın: audit alanları (Kaydeden/Güncelleyen) " +
            "yalnızca asenkron AuthenticationStateProvider çağrısıyla güvenilir şekilde doldurulabilir.");

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await UygulaAuditBilgisiAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task UygulaAuditBilgisiAsync()
    {
        if (!ChangeTracker.Entries<AuditableEntity>().Any(e => e.State is EntityState.Added or EntityState.Modified))
            return;

        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var kullanici = authState.User?.Identity?.Name ?? "system";
        var simdi = DateTime.Now;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.KayitTarihi = simdi;
                entry.Entity.Kaydeden = kullanici;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.GuncellemeTarihi = simdi;
                entry.Entity.Guncelleyen = kullanici;
            }
        }
    }
}
