using HotChocolate;

namespace GraphqlBff.Schema;

internal static class Ids
{
    /// <summary>Los ids de las APIs son GUID: se valida el formato antes de llamar a nadie.</summary>
    public static Guid Parse(string id, string entidad)
    {
        if (!Guid.TryParse(id, out var guid))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage($"El id del {entidad} debe ser un GUID valido.")
                .SetCode("BAD_USER_INPUT")
                .Build());
        }

        return guid;
    }
}
