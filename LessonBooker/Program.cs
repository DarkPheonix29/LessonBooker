using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using LBCore.Interfaces;
using LBCore.Managers;
using LBRepository.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LBRepository;
using Microsoft.Extensions.Logging;
using BLL.Firebase;

var builder = WebApplication.CreateBuilder(args);

// --- Firebase Initialization ---
FirebaseConfig.InitializeFirebase();


// Register FirebaseAuth for DI
builder.Services.AddScoped<FirebaseAuth>(_ => FirebaseAuth.DefaultInstance);

// --- Add DbContext Configuration ---
var connectionString = builder.Configuration.GetConnectionString("LocalPostgres");
var env = builder.Environment.EnvironmentName;

if (env == "Development")
{
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseNpgsql(connectionString));
}
else
{
	// ?? TODO: Add production DB connection here
	throw new InvalidOperationException("Production database configuration not defined.");
}

// --- Dependency Injection for Repositories and Managers ---
builder.Services.AddScoped<IFirebaseAccountRepos, FirebaseAccountRepos>();
builder.Services.AddScoped<IBookingRepos, BookingRepos>();
builder.Services.AddScoped<IAvailabilityRepos, AvailabilityRepos>();
builder.Services.AddScoped<FirebaseManager>();
builder.Services.AddScoped<AccountManager>();
builder.Services.AddScoped<IFirebaseKeyRepos, FirebaseKeyRepos>();
builder.Services.AddScoped<IProfileRepos, ProfileRepos>();

// --- Authentication and Authorization ---
builder.Services.AddAuthentication("Firebase")
	.AddCookie("Firebase");

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	});
});

// --- Add Controllers and Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lesson Booker API", Version = "v1" });
});

// --- Build and Configure the HTTP Request Pipeline ---
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
