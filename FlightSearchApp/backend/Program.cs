using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Flight Search API", 
        Version = "v1",
        Description = "API for searching flights using natural language"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:5173") // Vite's default port
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Configure Semantic Kernel
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddOpenAIChatCompletion(
        "gpt-4o-mini",
        "API_KEY_HERE" // Replace with your actual API key or use configuration
    );
    return kernelBuilder.Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flight Search API v1");
        c.RoutePrefix = "swagger"; // This makes it available at /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();

// Define the flight search prompt
string flightSearchPrompt = @"You are a helpful travel assistant.
                            Given the user's request, extract and respond with a list of 2–3 sample flights.
                            Format the response in this way:
                            Flights:
                            1. [Airline], [Departure Time] → [Arrival Time], [Duration]
                            2. ...
                            User Request: {{$input}}
                            Response:";

// Define the endpoint
app.MapPost("/api/flights/search", async (SearchRequest request, Kernel kernel) =>
{
    try
    {
        var flightSearchFunction = kernel.CreateFunctionFromPrompt(
            flightSearchPrompt,
            functionName: "FlightSearch",
            description: "Simulates flight search based on natural language."
        );

        var result = await kernel.InvokeAsync(flightSearchFunction, new KernelArguments
        {
            ["input"] = request.Query
        });

        return Results.Ok(new { flights = result.GetValue<string>() });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("SearchFlights")
.WithOpenApi(operation =>
{
    operation.Summary = "Search for flights";
    operation.Description = "Search for flights based on natural language request";
    return operation;
});

// Configure Kestrel to listen on port 5000
app.Urls.Add("http://localhost:5000");

// Add a simple root endpoint to verify the API is running
app.MapGet("/", () => "Flight Search API is running! Go to /swagger to see the documentation.");

app.Run();

// Define the search request model
public record SearchRequest(string Query);