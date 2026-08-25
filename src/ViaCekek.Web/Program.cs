using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ViaCekek.Web.Components;
using ViaCekek.Web.Components.Account;
using ViaCekek.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// IIS'te "Load User Profile" ayarına bağımlı kalmadan anahtar kalıcılığını
// garanti eder: bu olmadan her app pool restart'ında yeni anahtar üretilir,
// mevcut auth cookie/antiforgery token'lar geçersiz kalır (sürekli login
// yenilenme döngüsüne sebep olur).
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "keys")))
    .SetApplicationName("ViaCekek");

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Identity'nin store'ları (UserStore/RoleStore) Scoped bir ApplicationDbContext
// bekler, o yüzden AddDbContext hâlâ duruyor. Ama sayfalarımız artık bunu
// doğrudan enjekte etmek yerine IDbContextFactory kullanıyor (aşağıda) — her
// işlemde kısa ömürlü, ayrı bir DbContext açar. Bunun sebebi: prerender +
// interaktif circuit aynı Scoped DbContext'i paylaşırsa "A second operation
// was started on this context instance..." hatası oluyordu (bkz. CLAUDE.md);
// önceki çözüm prerender'ı tamamen kapatmaktı ama bu, WebSocket/SignalR
// bağlantısı kurulana kadar sayfa gövdesini boş bırakıyor — VPN üzerinden
// bağlanan mobil cihazlarda bu bağlantı hiç kurulamayınca sayfa kalıcı olarak
// boş kalıyordu. IDbContextFactory ile her sayfa kendi kısa ömürlü context'ini
// açıp kapattığı için prerender güvenle tekrar açılabiliyor.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// AddDbContext + AddDbContextFactory'yi aynı context için birlikte kullanmak
// DI doğrulama hatası veriyor (Singleton factory, Scoped DbContextOptions
// tüketemez) — bunun yerine Identity'nin ihtiyaç duyduğu Scoped
// ApplicationDbContext, factory'den üretilerek kaydediliyor (resmi
// önerilen kombine desen).
builder.Services.AddScoped<ApplicationDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Roller ve ilk yönetici hesabını uygulama başlarken oluştur/garanti et.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var rol in Roller.Hepsi)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    var ilkYonetici = await userManager.FindByNameAsync("kemalyilmaz@viadmc.com");
    if (ilkYonetici is not null && !await userManager.IsInRoleAsync(ilkYonetici, Roller.Yonetici))
    {
        await userManager.AddToRoleAsync(ilkYonetici, Roller.Yonetici);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
