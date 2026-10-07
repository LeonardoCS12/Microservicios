using GraphqlBff.Clients;
using GraphqlBff.Schema;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());

    // HU-03: permite leer el header Authorization de la peticion entrante
    // para reenviarlo a los microservicios REST.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddTransient<BearerTokenHandler>();

    // HU-03: cliente HTTP tipado hacia el microservicio de Productos.
    builder.Services.AddHttpClient<ProductsClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Services:Products"]!);
        })
        .AddHttpMessageHandler<BearerTokenHandler>();

    // HU-02: servidor GraphQL (el schema se genera a partir de las clases C#). HU-04: se agrega el tipo Mutation.
    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>();

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.MapGraphQL("/graphql");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "GraphQL BFF fallo al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}
