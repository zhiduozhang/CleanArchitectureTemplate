using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Identity;
public static class IdentityServerBuilderExtensions
{
    public static IIdentityServerBuilder LoadSigningCredentialFrom(this IIdentityServerBuilder builder, string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            builder.AddSigningCredential(new X509Certificate2(path));
        }
        else
        {
            builder.AddDeveloperSigningCredential();
        }

        return builder;
    }
}