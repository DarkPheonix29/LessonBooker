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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Firebase Initialization ---
FirebaseConfig.InitializeFirebase();

// Register FirebaseAuth for DI
builder.Services.AddScoped<FirebaseAuth>(_ => FirebaseAuth.DefaultInstance);

// --- Add DbContext Configuration ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException("Database connection string is not configured.");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(connectionString));

// --- Dependency Injection for Repositories and Managers ---
builder.Services.AddScoped<IFirebaseAccountRepos, FirebaseAccountRepos>();
builder.Services.AddScoped<IBookingRepos, BookingRepos>();
builder.Services.AddScoped<IAvailabilityRepos, AvailabilityRepos>();
builder.Services.AddScoped<FirebaseManager>();
builder.Services.AddScoped<AccountManager>();
builder.Services.AddScoped<CalendarManager>();
builder.Services.AddScoped<IFirebaseKeyRepos, FirebaseKeyRepos>();
builder.Services.AddScoped<IProfileRepos, ProfileRepos>();

// --- Authentication and Authorization ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.Authority = "https://securetoken.google.com/lessonbooker-8664a";
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = "https://securetoken.google.com/lessonbooker-8664a",
			ValidateAudience = true,
			ValidAudience = "lessonbooker-8664a",
			ValidateLifetime = true
		};
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	});
});

// --- Add Controllers and Swagger ---
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lesson Booker API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("https://lessonbooker-8664a.web.app")
			  .AllowAnyHeader()
			  .AllowAnyMethod()
			  .AllowCredentials(); // <-- Add this line
	});
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

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LessonBooker.Hubs.CalendarHub>("/calendarHub");

app.Run();
