using Booking.Controllers;
using Booking.EF;
using Booking.Models;
using Booking.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Net.payOS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContext"));
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cấu hình Identity
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    //options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    //options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // User
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // SignIn
    options.SignIn.RequireConfirmedEmail = true; // Xác thực email
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/customer/login";
    options.LogoutPath = "/customer/logout";
    options.AccessDeniedPath = "/khong-co-quyen";
});

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        var conf = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = conf["ClientId"] ?? string.Empty;
        options.ClientSecret = conf["ClientSecret"] ?? string.Empty;
        //options.CallbackPath = "/signin-google";
        options.Events = new OAuthEvents
        {
            // context.Failure chứa thông tin lỗi 

            OnRemoteFailure = context =>
            {
                // Chuyển hướng đến trang đăng nhập kèm thông báo lỗi error
                context.Response.Redirect("/customer/login?error=error-external-login");
                context.HandleResponse(); // Xử lý sự kiện và ngăn chặn lỗi tiếp tục
                return Task.CompletedTask;
            }
        };
    });

// AddTransient: Dịch vụ được tạo mới mỗi khi nó được yêu cầu.
// AddScoped: Dịch vụ được tạo một lần cho mỗi yêu cầu HTTP.
// AddSingleton: Dịch vụ được tạo một lần duy nhất.
builder.Services.AddScoped<SendMailService>();
builder.Services.AddScoped<CloudinaryController>();
builder.Services.AddScoped<PaymentController>();
builder.Services.AddScoped<Cloudinary>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var account = new Account(
        configuration["Cloudinary:CloudName"],
        configuration["Cloudinary:ApiKey"],
        configuration["Cloudinary:ApiSecret"]
    );
    return new Cloudinary(account);
});
builder.Services.AddScoped<PayOS>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var clientKey = configuration["PayOS:ClientId"] ?? string.Empty;
    var apiKey = configuration["PayOS:ApiKey"] ?? string.Empty;
    var checksumKey = configuration["PayOS:ChecksumKey"] ?? string.Empty;

    return new PayOS(clientKey, apiKey, checksumKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.Run();
