using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductManagement.Api.BackgroundServices;
using ProductManagement.Api.BuisnessLogic;
using ProductManagement.Api.Data;
using System.Runtime.CompilerServices;
using System.Text;
[assembly: InternalsVisibleTo("ProductManagement.Tests.Integration")]


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IProductService, ProductService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

// register windows auth strategy
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "DecideScheme";
}
)
    .AddPolicyScheme("DecideScheme", "JWT or Windows Auth", options =>
    {

        options.ForwardDefaultSelector = context =>
        {
            string? authHeader = context.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return NegotiateDefaults.AuthenticationScheme;
        };
    }
    )
    .AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "valid_issuer",
            ValidAudience = "product-api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretBackEndKey123456789!"))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy => policy.RequireAuthenticatedUser() );
}
);

// start the SyncWorker only in dev and staging/prod mode for testing we do not need that.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddHostedService<SyncWorker>();
}


var app = builder.Build();

// Configure the HTTP request pipeline for dev env.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}else{
    app.UseExceptionHandler("/Error");
    app.UseHsts(); // use encrypted http connection/protocol
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public partial class Program { }