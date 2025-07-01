using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Password_Generator.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
    });

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication, authorization, and session middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

void SeedDemoData(ApplicationDbContext context)
{
    if (context.Password_Generator_Users.Any(u => u.Email == "demo@demo.com"))
        return;

    var demoUser = new Password_Generator.Models.User_Password_Generator
    {
        FirstName = "Fred",
        LastName = "Smith",
        Email = "demo@demo.com",
        HashedPassword = BCrypt.Net.BCrypt.HashPassword("password"),
        PhoneNumber = "123-456-7890",
        ResetQuestion1 = Password_Generator.Models.SecurityQuestion.WhatIsYourMotherMaidenName,
        ResetAnswer1 = "RandomAnswer1",
        ResetQuestion2 = Password_Generator.Models.SecurityQuestion.WhatWasTheNameOfYourFirstPet,
        ResetAnswer2 = "RandomAnswer2"
    };

    context.Password_Generator_Users.Add(demoUser);
    context.SaveChanges();

    var vendors = new List<Password_Generator.Models.VendorPassword>
    {
        new Password_Generator.Models.VendorPassword
        {
            VendorName = "Netflix",
            Url = "https://www.netflix.com",
            CurrentPassword = Password_Generator.Helpers.EncryptionHelper.Encrypt(GenerateRandomPassword(12, true)),
            UserId = demoUser.Id,
            Category = Password_Generator.Models.PasswordCategory.Entertainment,
            Username = "fred.netflix"
        },
        new Password_Generator.Models.VendorPassword
        {
            VendorName = "Gmail",
            Url = "https://mail.google.com",
            CurrentPassword = Password_Generator.Helpers.EncryptionHelper.Encrypt(GenerateRandomPassword(12, true)),
            UserId = demoUser.Id,
            Category = Password_Generator.Models.PasswordCategory.Email,
            Username = "fred@gmail.com"
        },
        new Password_Generator.Models.VendorPassword
        {
            VendorName = "Amazon",
            Url = "https://www.amazon.com",
            CurrentPassword = Password_Generator.Helpers.EncryptionHelper.Encrypt(GenerateRandomPassword(12, true)),
            UserId = demoUser.Id,
            Category = Password_Generator.Models.PasswordCategory.Shopping,
            Username = "fred.amazon"
        }
    };

    context.VendorPasswords.AddRange(vendors);
    context.SaveChanges();
}

string GenerateRandomPassword(int length, bool includeSpecialChars)
{
    const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    const string specialChars = "!@#$%^&*()_+[]{}|;:,.<>?";
    var chars = validChars + (includeSpecialChars ? specialChars : string.Empty);
    var random = new Random();
    return new string(Enumerable.Range(0, length).Select(x => chars[random.Next(chars.Length)]).ToArray());
}


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    SeedDemoData(dbContext);
}

app.Run();

