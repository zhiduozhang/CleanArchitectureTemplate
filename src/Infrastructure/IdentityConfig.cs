using Duende.IdentityServer.Models;

public static class IdentityConfig
{
    public static IdentityResource[] IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static ApiScope[] ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("general_api.read"),
            new ApiScope("general_api.write"),
            new ApiScope("general_api.delete"),
        };

    public static ApiResource[] ApiResources => new[]
    {
        new ApiResource("general_api")
        {
            Scopes = new List<string> { "general_api"}
        }
    };

    public static Client[] Clients =>
        new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,

                ClientSecrets = { new Secret("my-secret".Sha256()) },
                AllowedScopes = { "general_api" }
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("my-secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                // List of URIs we can redirect back to.
                RedirectUris = { "http://localhost:5173/" },
                //FrontChannelLogoutUri = "https://localhost:44447/signout-oidc",
                //PostLogoutRedirectUris = { "https://localhost:44447/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "general_api", "WebUIAPI"},
            },
        };
}