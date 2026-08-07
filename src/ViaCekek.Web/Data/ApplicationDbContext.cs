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
    public DbSet<KisiBelge> KisiBelgeleri => Set<KisiBelge>();
    public DbSet<AracBelge> AracBelgeleri => Set<AracBelge>();
    public DbSet<KisiBelgeKontrol> KisiBelgeKontrolleri => Set<KisiBelgeKontrol>();
    public DbSet<AracBelgeKontrol> AracBelgeKontrolleri => Set<AracBelgeKontrol>();
    public DbSet<CekekTakip> CekekTakipleri => Set<CekekTakip>();

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

        builder.Entity<Kisi>()
            .Property(k => k.KvkkOnayDurumu)
            .HasConversion<string>()
            .HasMaxLength(20);

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

        // KisiBelgeKontrolleri / AracBelgeKontrolleri artık kişiye/araca
        // bağlı GÜNCEL durum: bir kişi/araç × bir belge kuralı = tek satır
        // (upsert edilir, log değil).
        builder.Entity<KisiBelgeKontrol>()
            .HasIndex(kbk => new { kbk.KisiId, kbk.KisiBelgeId })
            .IsUnique();

        builder.Entity<AracBelgeKontrol>()
            .HasIndex(abk => new { abk.AracId, abk.AracBelgeId })
            .IsUnique();

        // CekekTakip silinse bile kişinin/aracın güncel belge durumu kalır,
        // yalnızca "hangi girişte teyit edildi" bağlantısı NULL'a düşer.
        builder.Entity<KisiBelgeKontrol>()
            .HasOne(kbk => kbk.CekekTakip)
            .WithMany()
            .HasForeignKey(kbk => kbk.CekekTakipId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AracBelgeKontrol>()
            .HasOne(abk => abk.CekekTakip)
            .WithMany()
            .HasForeignKey(abk => abk.CekekTakipId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CekekTakip>()
            .Property(c => c.ZiyaretSebebi)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Bir CekekTakip satırı ya kişi ya araç girişidir, ikisi birden olamaz.
        builder.Entity<CekekTakip>().ToTable(t => t.HasCheckConstraint(
            "CK_CekekTakipleri_KisiVeyaArac",
            "([KisiId] IS NOT NULL AND [AracId] IS NULL) OR ([KisiId] IS NULL AND [AracId] IS NOT NULL)"));

        // Kisi/Arac/Tekne silinse bile geçmiş giriş/çıkış kayıtları kalır
        // (snapshot alanları zaten bağımsız), yalnızca bağlantı NULL'a düşer.
        builder.Entity<CekekTakip>()
            .HasOne(c => c.Kisi)
            .WithMany()
            .HasForeignKey(c => c.KisiId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CekekTakip>()
            .HasOne(c => c.Arac)
            .WithMany()
            .HasForeignKey(c => c.AracId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CekekTakip>()
            .HasOne(c => c.Tekne)
            .WithMany()
            .HasForeignKey(c => c.TekneId)
            .OnDelete(DeleteBehavior.SetNull);
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
