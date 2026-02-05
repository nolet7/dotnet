var builder = WebApplication.CreateBuilder(args);

// Explicit HTTP only (NO HTTPS)
builder.WebHost.UseUrls("http://0.0.0.0:5001");

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger (safe)
app.UseSwagger();
app.UseSwaggerUI();

// NO HTTPS redirection
// NO Authorization middleware needed here

app.MapControllers();

app.Run();

