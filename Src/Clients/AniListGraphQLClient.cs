using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace Tsundoku.Clients;

/// <summary>
/// A specialized GraphQL HTTP client configured for the AniList API endpoint.
/// </summary>
public sealed class AniListGraphQLClient : GraphQLHttpClient
{
    /// <summary>
    /// Initializes a new instance of <see cref="AniListGraphQLClient"/> using the provided <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured with the AniList base address.</param>
    public AniListGraphQLClient(HttpClient httpClient)
        : base(new GraphQLHttpClientOptions
        {
            EndPoint = httpClient.BaseAddress!
        }, new SystemTextJsonSerializer(), httpClient)
    {
    }
}
