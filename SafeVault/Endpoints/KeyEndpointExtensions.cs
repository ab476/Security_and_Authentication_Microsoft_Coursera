using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace SafeVault.Endpoints;

public static class KeyEndpointExtensions
{
    /// <summary>
    /// Maps endpoints for generating keys used by JWT (HMAC), AES (symmetric) and RSA (asymmetric).
    /// IMPORTANT: These endpoints return private material. Ensure they are protected with Authorization
    /// and served only over TLS in production. Consider writing keys directly to a secure key store instead
    /// of returning them over HTTP.
    /// </summary>
    public static void MapKeyEndpoints(this IEndpointRouteBuilder app)
    {
        // Generate an HMAC key suitable for signing JWTs (HMAC-SHA256)
        app.MapPost("/api/keys/jwt", (JwtKeyRequest req) =>
        {
            var keyBytes = new byte[req.KeySizeBits / 8];
            RandomNumberGenerator.Fill(keyBytes);
            var base64Key = Convert.ToBase64String(keyBytes);

            var resp = new JwtKeyResponse
            {
                Algorithm = "HS256",
                KeyBase64 = base64Key,
                KeySizeBits = req.KeySizeBits
            };

            return Results.Ok(resp);
        })
        .WithTags("keys")
        .WithName("CreateJwtHmacKey");

        // Generate AES key + IV
        app.MapPost("/api/keys/aes", (AesKeyRequest req) =>
        {
            using var aes = Aes.Create();
            aes.KeySize = req.KeySizeBits; // 128 / 192 / 256
            aes.GenerateKey();
            aes.GenerateIV();

            var resp = new AesKeyResponse
            {
                Algorithm = $"AES-{aes.KeySize}",
                KeyBase64 = Convert.ToBase64String(aes.Key),
                IvBase64 = Convert.ToBase64String(aes.IV),
                KeySizeBits = aes.KeySize
            };

            return Results.Ok(resp);
        })
        .WithTags("keys")
        .WithName("CreateAesKey");

        // Generate RSA key pair and return PEM encoded keys
        app.MapPost("/api/keys/rsa", (RsaKeyRequest req) =>
        {
            // enforce allowed sizes
            var size = req.KeySizeBits switch
            {
                2048 => 2048,
                3072 => 3072,
                4096 => 4096,
                _ => 2048
            };

            using var rsa = RSA.Create(size);

            var privatePkcs8 = rsa.ExportPkcs8PrivateKey(); // PKCS#8
            var publicSpki = rsa.ExportSubjectPublicKeyInfo(); // X.509 SubjectPublicKeyInfo


            var resp = new RsaKeyResponse
            {
                KeySizeBits = size,
                PrivateKeyPem = Convert.ToBase64String(privatePkcs8),
                PublicKeyPem = Convert.ToBase64String(publicSpki)
            };

            return Results.Ok(resp);
        })
        .WithTags("keys")
        .WithName("CreateRsaKeyPair");
    }

    #region Helpers & DTOs

    // Request/Response DTOs
    public sealed record JwtKeyRequest(int KeySizeBits = 256);

    public sealed record JwtKeyResponse
    {
        public string Algorithm { get; init; } = string.Empty;
        public string KeyBase64 { get; init; } = string.Empty;
        public int KeySizeBits { get; init; }
    }

    public sealed record AesKeyRequest(int KeySizeBits = 256);

    public sealed record AesKeyResponse
    {
        public string Algorithm { get; init; } = string.Empty;
        public string KeyBase64 { get; init; } = string.Empty;
        public string IvBase64 { get; init; } = string.Empty;
        public int KeySizeBits { get; init; }
    }

    public sealed record RsaKeyRequest(int KeySizeBits = 2048);

    public sealed record RsaKeyResponse
    {
        public int KeySizeBits { get; init; }
        public string PrivateKeyPem { get; init; } = string.Empty;
        public string PublicKeyPem { get; init; } = string.Empty;
    }

    #endregion
}
