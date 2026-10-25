using System.Text.Json;
using GraphqlBff.Clients;
using GraphqlBff.Schema;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GraphqlBff.Tests;

/// <summary>
/// Levanta el schema GraphQL real del BFF (mismos tipos Query y Mutation que Program.cs) pero con
/// los clientes de Productos y Usuarios sustituidos por dobles (Moq). Asi se ejecutan queries y
/// mutations de verdad, sin red ni base de datos.
/// Si cambias el registro de AddGraphQLServer() en Program.cs, cambialo tambien aqui.
/// </summary>
internal sealed class BffHarness
{
    public Mock<IProductsClient> Products { get; } = new();
    public Mock<IUsersClient> Users { get; } = new();

    /// <summary>Ejecuta una operacion GraphQL y devuelve la respuesta como JSON ({ data, errors }).</summary>
    public async Task<JsonDocument> RunAsync(string operation)
    {
        var executor = await new ServiceCollection()
            .AddSingleton(Products.Object)
            .AddSingleton(Users.Object)
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .BuildRequestExecutorAsync();

        var result = await executor.ExecuteAsync(operation);
        return JsonDocument.Parse(result.ToJson());
    }
}
