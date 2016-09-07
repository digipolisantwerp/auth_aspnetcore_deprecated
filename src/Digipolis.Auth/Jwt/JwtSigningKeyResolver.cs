using Digipolis.Auth.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Digipolis.Auth.Jwt
{
    public class JwtSigningKeyResolver : IJwtSigningKeyResolver
    {
        private readonly IMemoryCache _cache;
        private readonly AuthOptions _options;
        private readonly MemoryCacheEntryOptions _cacheOptions;
        private readonly HttpClient _client;
        private readonly bool _cachingEnabled;
        private const string CACHE_KEY = "JwtSigningKey";
        private readonly ILogger<JwtSigningKeyResolver> _logger;

        public JwtSigningKeyResolver(IMemoryCache cache, IOptions<AuthOptions> options, HttpMessageHandler handler, ILogger<JwtSigningKeyResolver> logger)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache), $"{nameof(cache)} cannot be null");
            if (options == null || options.Value == null) throw new ArgumentNullException(nameof(options), $"{nameof(options)} cannot be null");
            if (handler == null) throw new ArgumentNullException(nameof(handler), $"{nameof(handler)} cannot be null");
            if (logger == null) throw new ArgumentNullException(nameof(logger), $"{nameof(logger)} cannot be null");

            _cache = cache;
            _options = options.Value;
            _client = new HttpClient(handler, true);
            _client.DefaultRequestHeaders.Add("apikey", _options.JwtSigningCertificateProviderApikey);
            _logger = logger;

            if (_options.JwtSigningKeyCacheDuration > 0)
            {
                _cachingEnabled = true;
                _cacheOptions = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = new TimeSpan(0, _options.JwtSigningKeyCacheDuration, 0) };
            }
        }

        public IEnumerable<SecurityKey> IssuerSigningKeyResolver(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            var jwt = securityToken as JwtSecurityToken;
            var x5uUrl = jwt?.Header["x5u"]?.ToString();

            if (x5uUrl == null)
                throw new NullReferenceException("No x5u header present in jwt token or invalid jwt token.");

            var key = GetSecurityKeyFromX5u(x5uUrl, true);
            
            return new List<SecurityKey> { key };
        }

        private SecurityKey GetSecurityKeyFromX5u(string x5uUrl, bool allowCached)
        {
            SecurityKey key = null;

            if (allowCached && _cachingEnabled)
            {
                key = _cache.Get(CACHE_KEY) as SecurityKey;

                if (key != null)
                    return key;
            }

            var response = _client.GetAsync(x5uUrl).Result;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Retreiving x5u certificate failed. Request url: {x5uUrl}, Response status code: {response.StatusCode}");

            var x5uResponse = response.Content.ReadAsAsync<X5uResponse>().Result;
            var x5u = x5uResponse.x5u;

            var cert = Encoding.UTF8.GetString(Convert.FromBase64String(x5u));

            key = TransformCertificateToSecurityKey(cert);

            if (_cachingEnabled)
                _cache.Set(CACHE_KEY, key, _cacheOptions);

            return key;
        }

        private static RsaSecurityKey TransformCertificateToSecurityKey(string cert)
        {
            cert = cert.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "");
            RSACryptoServiceProvider rsa = DecodeX509PublicKey(Convert.FromBase64String(cert));

            var rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(false));

            return rsaSecurityKey;
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        public static RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            MemoryStream mem = new MemoryStream(x509key);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            byte bt = 0;
            ushort twobytes = 0;

            try
            {

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                seq = binr.ReadBytes(15);       //read the Sequence OID
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x00)     //expect null byte next
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();    //advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();   //advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte(); //advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {   //if first byte (highest order) of modulus is zero, don't include it
                    binr.ReadByte();    //skip this null byte
                    modsize -= 1;   //reduce modulus buffer size by 1
                }

                byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                    return null;
                int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                byte[] exponent = binr.ReadBytes(expbytes);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters();
                RSAKeyInfo.Modulus = modulus;
                RSAKeyInfo.Exponent = exponent;
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Dispose(); }
        }
    }
}
