using AttendanceManagement.Api.Configurations;
using AttendanceManagement.Api.Handlers;
using AttendanceManagement.Application;
using AttendanceManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersConfiguration();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddCompressionConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// ProblemDetails (RFC 9457) como formato único de erro. O traceId correlaciona
// a resposta do cliente com a linha de log do servidor.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// A ordem do pipeline importa. UseExceptionHandler primeiro, para capturar o que
// vier dos middlewares seguintes; autenticação/autorização antes do MapControllers.
app.UseExceptionHandler();
app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseCors(CorsConfiguration.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

// Documentação só fora de Produção.
if (!app.Environment.IsProduction())
{
    app.UseOpenApiConfiguration();

    // A raiz não tem endpoint; manda para a documentação.
    app.MapGet("/", () => Results.Redirect("/scalar"))
        .ExcludeFromDescription();
}

app.UseHealthCheckConfiguration();
app.MapControllers();

await app.RunAsync();
