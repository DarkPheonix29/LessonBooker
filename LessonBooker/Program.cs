using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using LBCore.Interfaces;
using LBCore.Managers;
using LBRepository.Repos;
using Microsoft.AspNetCore.Builder.Extensions;

var builder = WebApplication.CreateBuilder(args);

var firebaseKeyPath = Environment.GetEnvironmentVariable("FIREBASE_KEY_PATH") ?? builder.Configuration["Firebase:ServiceAccountKeyPath"];
if (string.IsNullOrEmpty(firebaseKeyPath) || !File.Exists(firebaseKeyPath))
{
	throw new FileNotFoundException($"Firebase service account key file not found at '{firebaseKeyPath}'.");
}

FirebaseApp.Create(new AppOptions
{
	Credential = GoogleCredential.FromFile(firebaseKeyPath)
});

builder.Services.AddScoped<FirebaseAuth>(_ => FirebaseAuth.DefaultInstance);

// Dependency Injection for Repositories and Managers
builder.Services.AddScoped<IFirebaseAccountRepos, FirebaseAccountRepos>();
builder.Services.AddScoped<FirebaseManager>();

builder.Services.AddAuthentication("Firebase")
	.AddCookie("Firebase"); // Use cookie-based authentication for user sessions

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAuthenticatedUser", policy =>
	{
		policy.RequireAuthenticatedUser();
	});
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
