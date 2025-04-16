using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using LBCore.Interfaces;
using LBCore.Managers;
using LBRepository.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System.IO;
using LBRepository;

var builder = WebApplication.CreateBuilder(args);

// --- Firebase Initialization ---
var firebaseKeyPath = Environment.GetEnvironmentVariable("FIREBASE_KEY_PATH") ?? builder.Configuration["Firebase:ServiceAccountKeyPath"];
if (string.IsNullOrEmpty(firebaseKeyPath) || !File.Exists(firebaseKeyPath))
{
	throw new FileNotFoundException($"Firebase service account key file not found at '{firebaseKeyPath}'");
}

// Initialize Firebase SDK with credentials
FirebaseApp.Create(new AppOptions
{
	Credential = GoogleCredential.FromFile(firebaseKeyPath)
});

builder.Services.AddScoped<FirebaseAuth>(_ => FirebaseAuth.DefaultInstance);

// --- Add DbContext Configuration ---
var connectionString = builder.Configuration.GetConnectionString("LocalPostgres");

// Determine which database to use: PostgreSQL for local or Cloud SQL for production
var env = builder.Environment.EnvironmentName;
if (env == "Development")
{
	builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseNpgsql(connectionString)); // PostgreSQL for local development
}


// --- Dependency Injection for Repositories and Managers ---
builder.Services.AddScoped<IFirebaseAccountRepos, FirebaseAccountRepos>();
builder.Services.AddScoped<IBookingRepos, BookingRepos>();
builder.Services.AddScoped<IAvailabilityRepos, AvailabilityRepos>();
builder.Services.AddScoped<FirebaseManager>();

// --- Authentication and Authorization ---
builder.Services.AddAuthentication("Firebase")
	.AddCookie("Firebase"); // Cookie-based authentication for user sessions

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	});
});

// --- Add Controllers and Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger for API documentation
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lesson Booker API", Version = "v1" });
});

var app = builder.Build();

// --- Apply Migrations at Startup ---
using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	try
	{
		context.Database.Migrate(); // Apply pending migrations automatically at startup
	}
	catch (Exception ex)
	{
		// Log migration error if any
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred applying the migrations.");
	}
}

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Enable Authentication Middleware
app.UseAuthorization();  // Enable Authorization Middleware

app.MapControllers();

app.Run();
