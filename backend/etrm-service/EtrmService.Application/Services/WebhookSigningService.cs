using System.Security.Cryptography;
using System.Text;

namespace EtrmService.Application.Services;

/// <summary>
/// Helper compartilhado para assinatura HMAC-SHA256 de payloads de webhook.
/// Gera um header no formato "sha256=&lt;hex&gt;" (padrão compatível com GitHub/Stripe).
/// A chave secreta nunca transita em texto claro no header.
/// </summary>
public static class WebhookSigningService
{
    public static string ComputeSignature(string secret, string payload)
    {
        if (string.IsNullOrEmpty(secret))
            return string.Empty;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}