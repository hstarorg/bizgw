using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;

namespace GatewayServer.ControlPlane.Auth
{
    /// <summary>
    /// Cookie 票据的固定密钥加密(AES-GCM),替代默认的 DataProtection 实现。
    /// 密钥经环境变量 AuthCookieKey 传入 —— 容器重建/多实例登录态天然一致,无 key ring 持久化。
    /// 取舍:放弃自动密钥轮换(换密钥 = 换 env 重启,全员重登);密钥泄漏可伪造任意角色 cookie,按 secret 管理。
    /// </summary>
    public sealed class FixedKeyTicketFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;

        private readonly byte[] key;

        public FixedKeyTicketFormat(byte[] key)
        {
            if (key.Length != 32)
                throw new ArgumentException("cookie 密钥必须是 32 字节(AuthCookieKey 配置其 base64)", nameof(key));
            this.key = key;
        }

        public string Protect(AuthenticationTicket data) => Protect(data, null);

        public string Protect(AuthenticationTicket data, string? purpose)
        {
            var plaintext = TicketSerializer.Default.Serialize(data);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSize];
            using var aes = new AesGcm(key, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag, PurposeBytes(purpose));

            var payload = new byte[NonceSize + ciphertext.Length + TagSize];
            nonce.CopyTo(payload, 0);
            ciphertext.CopyTo(payload, NonceSize);
            tag.CopyTo(payload, NonceSize + ciphertext.Length);
            return WebEncoders.Base64UrlEncode(payload);
        }

        public AuthenticationTicket? Unprotect(string? protectedText) => Unprotect(protectedText, null);

        public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
        {
            if (string.IsNullOrEmpty(protectedText)) return null;
            try
            {
                var payload = WebEncoders.Base64UrlDecode(protectedText);
                if (payload.Length < NonceSize + TagSize) return null;

                var nonce = payload.AsSpan(0, NonceSize);
                var tag = payload.AsSpan(payload.Length - TagSize);
                var ciphertext = payload.AsSpan(NonceSize, payload.Length - NonceSize - TagSize);
                var plaintext = new byte[ciphertext.Length];
                using var aes = new AesGcm(key, TagSize);
                aes.Decrypt(nonce, ciphertext, tag, plaintext, PurposeBytes(purpose));
                return TicketSerializer.Default.Deserialize(plaintext);
            }
            catch
            {
                // 篡改 / 密钥不符 / 格式损坏 → 视为未登录(交由认证管线走 401)
                return null;
            }
        }

        private static byte[]? PurposeBytes(string? purpose)
            => string.IsNullOrEmpty(purpose) ? null : Encoding.UTF8.GetBytes(purpose);
    }
}
