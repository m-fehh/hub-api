using Hub.Infrastructure.Extensions;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Infrastructure.Web
{
    /// <summary>
    /// Validates access tokes that have been submitted as part of a request.
    /// </summary>
    public interface IAccessTokenProvider
    {
        AccessTokenResult ValidateToken(string token, string issuerToken = null);

        AccessTokenResult ValidateExternalToken(string token, string externalIssuerToken);

        string GenerateToken(IEnumerable<Claim> claims, double expiryInMinutes = 30, string issuerToken = null);

        Claim RetriveTokenData(AccessTokenResult tokenResult, string claimKeyName);

        void ValidateTokenStatus(AccessTokenResult tokenResult);
    }


    /// <summary>
    /// Validates a incoming request and extracts any <see cref="ClaimsPrincipal"/> contained within the bearer token.
    /// </summary>
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AUTH_HEADER_NAME = "Authorization";
        private const string BEARER_PREFIX = "Bearer ";

        /// <summary>
        /// Valida um token gerado fora do sistema ELOS
        /// </summary>
        /// <param name="token">Token a ser validado</param>
        /// <param name="externalIssuerToken">Chave de assinatura do emissor</param>
        /// <returns></returns>
        public AccessTokenResult ValidateExternalToken(string token, string externalIssuerToken)
        {
            try
            {
                // Create the parameters
                var tokenParams = new TokenValidationParameters()
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(externalIssuerToken))
                };

                IdentityModelEventSource.ShowPII = true;

                // Validate the token
                var handler = new JwtSecurityTokenHandler();

                ClaimsPrincipal result = null;

                if (handler.CanValidateToken)
                {
                    result = handler.ValidateToken(token, tokenParams, out var securityToken);
                }

                return AccessTokenResult.Success(result);
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }

        /// <summary>
        /// Valida o token JWT com base na configuração <c>IssuerToken</c>.
        /// </summary>
        /// <param name="token">Token a ser validado</param>
        /// <param name="issuerToken">Chave de assinatura do emissor (se null, usará a default do sistema (config IssuerToken)</param>
        /// <returns>Classe <see cref="AccessTokenResult"/> que irá encapsular o resultado + os dados do token (<see cref="ClaimsPrincipal"/>)</returns>
        public AccessTokenResult ValidateToken(string token, string issuerToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(issuerToken))
                {
                    issuerToken = Engine.AppSettings["IssuerToken"];
                }

                // Create the parameters
                var tokenParams = new TokenValidationParameters()
                {
                    RequireSignedTokens = true,
                    ValidAudience = "aud",
                    ValidateAudience = true,
                    ValidIssuer = "systemHub",
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(issuerToken))
                };

                IdentityModelEventSource.ShowPII = true;

                // Validate the token
                var handler = new JwtSecurityTokenHandler();

                var result = handler.ValidateToken(token, tokenParams, out var securityToken);

                return AccessTokenResult.Success(result);
            }
            catch (SecurityTokenExpiredException)
            {
                return AccessTokenResult.Expired();
            }
            catch (Exception ex)
            {
                return AccessTokenResult.Error(ex);
            }
        }

        /// <summary>
        /// Gera token JWT para autenticação, usando a configuração <c>IssuerToken</c> para assinar.
        /// </summary>
        /// <param name="claims">Lista de claims (dados) que serão encapsulados no token</param>
        /// <param name="expiryInMinutes">Tempo de expiração do token</param>
        /// <param name="issuerToken">Chave de assinatura do emissor (se null, usará a default do sistema (config IssuerToken)</param>
        /// <returns></returns>
        public string GenerateToken(IEnumerable<Claim> claims, double expiryInMinutes = 30, string issuerToken = null)
        {
            if (string.IsNullOrEmpty(issuerToken))
            {
                issuerToken = Engine.AppSettings["IssuerToken"];
            }

            byte[] key = Convert.FromBase64String(issuerToken);

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                SigningCredentials = new SigningCredentials(securityKey,
                SecurityAlgorithms.HmacSha256Signature),
                Audience = "aud",
                Issuer = "systemHub"
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Obtem um <see cref="Claim"/> específico de um objeto com um token encapsulado.
        /// </summary>
        /// <param name="tokenResult">Objeto que contem o token encapsulado (utilize o método <see cref="ValidateToken(string, string)"/> para obter o resultado do token.</param>
        /// <param name="claimKeyName">Chave do claim que deseja-se obter</param>
        /// <returns></returns>
        public Claim RetriveTokenData(AccessTokenResult tokenResult, string claimKeyName)
        {
            return tokenResult.Principal?.Claims.FirstOrDefault(f => f.Type.Equals(claimKeyName));
        }

        /// <summary>
        /// Método comum para validação do resultado de um token
        /// </summary>
        /// <param name="tokenResult"></param>
        /// <exception cref="BusinessException"></exception>
        public virtual void ValidateTokenStatus(AccessTokenResult tokenResult)
        {
            if (tokenResult.Status != AccessTokenStatus.Valid)
            {
                throw new BusinessException("Token inválido");
            }
        }
    }
}
