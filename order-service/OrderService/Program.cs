var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.MapPost("/orders", async () =>
{
    using var client = new HttpClient();

    var inventory = await client.PostAsync("http://inventoryservice:5001/check", null);
    if (!inventory.IsSuccessStatusCode)
        return Results.BadRequest("Inventory failed");

    var payment = await client.PostAsync("http://paymentservice:5002/pay", null);
    if (!payment.IsSuccessStatusCode)
        return Results.BadRequest("Payment failed");

    return Results.Ok("Order completed");
});

app.Run();
