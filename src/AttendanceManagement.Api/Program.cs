using AttendanceManagement.Api.Configurations;
using AttendanceManagement.Api.Handlers;
using AttendanceManagement.Application;
using AttendanceManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Composition root: e AQUI, e so aqui, que as camadas se encontram.
//
// Note a direcao das dependencias. A Api chama AddInfrastructure uma unica vez;
// nenhum controller, service ou entidade sabe que PostgreSQL existe. Trocar de
// banco significa reescrever o projeto Infrastructure e nada mais.
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------------------------------------------------------------------------
// Servicos da camada web.
// ---------------------------------------------------------------------------
builder.Services.AddControllersConfiguration();
builder.Services.AddOpenApiConfiguration();
builder.Services.AddCompressionConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);

// ProblemDetails (RFC 9457) como formato unico de erro da API.
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        // traceId correlaciona a resposta do cliente com a linha de log do
        // servidor. Sem isso, "deu erro" e impossivel de investigar.
        context.ProblemDetails.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline. A ordem importa mais do que parece.
// ---------------------------------------------------------------------------

// Primeiro de todos: so assim ele captura excecao dos middlewares seguintes.
app.UseExceptionHandler();

// Antes do MVC, para que a resposta gerada la na frente ja saia comprimida.
app.UseResponseCompression();

app.UseHttpsRedirection();
app.UseCors(CorsConfiguration.PolicyName);

// Documentacao so fora de Producao. Um deploy publico nao deveria expor o
// mapa completo da API sem necessidade; para liberar, mova para fora do if.
if (!app.Environment.IsProduction())
{
    app.UseOpenApiConfiguration();
}

app.UseHealthCheckConfiguration();
app.MapControllers();

await app.RunAsync();
