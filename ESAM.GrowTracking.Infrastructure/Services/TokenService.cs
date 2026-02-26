using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Commons.Types;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Constants;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly ILogger<TokenService> _logger;
        private readonly JwtSetting _jwtSetting;
        private readonly SigningCredentials _signingCredentials;

        public TokenService(ILogger<TokenService> logger, IOptions<JwtSetting> options)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo.");
            Guard.AgainstNull(options, $"{nameof(options)} no puede ser nulo.");
            Guard.AgainstNull(options.Value, $"{nameof(options.Value)} no puede ser nulo.");
            Guard.AgainstNullOrWhiteSpace(options.Value.Issuer, $"{nameof(options.Value.Issuer)} debe proporcionarse.");
            Guard.AgainstNullOrWhiteSpace(options.Value.Audience, $"{nameof(options.Value.Audience)} debe proporcionarse.");
            Guard.AgainstNullOrWhiteSpace(options.Value.SecretKey, $"{nameof(options.Value.SecretKey)} debe proporcionarse.");
            Guard.Against(options.Value.SecretKey.Length < 32, $"{nameof(options.Value.SecretKey)} debe tener al menos 256 bits (32 bytes) de longitud.");
            _logger = logger;
            _jwtSetting = options.Value;
            _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SecretKey)), SecurityAlgorithms.HmacSha256);
        }

        public AccessTokenDTO GenerateAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, bool isPersistent, AccessTokenType accessTokenType, DateTime utcNow, int lifetimeMinutes)
        {
            Guard.Against(userId <= 0, $"{nameof(userId)} debe ser mayor a cero.");
            Guard.AgainstNullOrWhiteSpace(securityStamp, $"{nameof(securityStamp)} no puede ser nulo o vacio.");
            Guard.Against(tokenVersion < 0, $"{nameof(tokenVersion)} debe ser mayor a cero.");
            Guard.Against(userDeviceId <= 0, $"{nameof(userDeviceId)} debe ser mayor a cero.");
            Guard.Against(lifetimeMinutes <= 0, $"{nameof(lifetimeMinutes)} debe ser mayor a cero.");
            var accessTokenExpiresAt = utcNow.AddMinutes(lifetimeMinutes);
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, jti),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Sub, userId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.SecurityStamp, securityStamp),
                new(CustomClaimConstant.TokenVersion, tokenVersion.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.UserDeviceId, userDeviceId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.IsPersistent, isPersistent.ToString(), ClaimValueTypes.Boolean),
                new(CustomClaimConstant.AccessTokenType, ((byte)accessTokenType).ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.AccessTokenExpiration, new DateTimeOffset(accessTokenExpiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = utcNow,
                Expires = accessTokenExpiresAt,
                SigningCredentials = _signingCredentials
            };
            var jsonWebTokenHandler = new JsonWebTokenHandler();
            try
            {
                var accessToken = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
                var accessTokenExpiresIn = (int)(accessTokenExpiresAt - utcNow).TotalSeconds;
                _logger.LogDebug("Generated JWT: UserId={UserId}, AccessTokenType={accessTokenType}.", userId, accessTokenType);
                return new AccessTokenDTO(accessToken, accessTokenExpiresIn, accessTokenExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT for UserId={UserId}", userId);
                throw new InfrastructureException("No se pudo generar el token de acceso temporary.", ex);
            }
        }

        public AccessTokenDTO GenerateAccessToken(int userId, string securityStamp, int tokenVersion, int userDeviceId, int userSessionId, AccessTokenType accessTokenType, DateTime utcNow, int lifetimeMinutes,
            int? workProfileId = null, int? roleId = null, int? campusId = null)
        {
            Guard.Against(userId <= 0, $"{nameof(userId)} debe ser mayor a cero.");
            Guard.AgainstNullOrWhiteSpace(securityStamp, $"{nameof(securityStamp)} no puede ser nulo o vacio.");
            Guard.Against(tokenVersion < 0, $"{nameof(tokenVersion)} debe ser mayor a cero.");
            Guard.Against(userDeviceId <= 0, $"{nameof(userDeviceId)} debe ser mayor a cero.");
            Guard.Against(userSessionId <= 0, $"{nameof(userSessionId)} debe ser mayor a cero.");
            Guard.Against(lifetimeMinutes <= 0, $"{nameof(lifetimeMinutes)} debe ser mayor a cero.");
            Guard.Against((workProfileId is not null) && (workProfileId <= 0), $"{nameof(workProfileId)} debe ser mayor a cero.");
            Guard.Against((roleId is not null) && (roleId <= 0), $"{nameof(roleId)} debe ser mayor a cero.");
            Guard.Against((campusId is not null) && (campusId <= 0), $"{nameof(campusId)} debe ser mayor a cero.");
            var accessTokenExpiresAt = utcNow.AddMinutes(lifetimeMinutes);
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, jti),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(utcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Sub, userId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.SecurityStamp, securityStamp),
                new(CustomClaimConstant.TokenVersion, tokenVersion.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.UserDeviceId, userDeviceId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.UserSessionId, userSessionId.ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.AccessTokenType, ((byte)accessTokenType).ToString(), ClaimValueTypes.Integer64),
                new(CustomClaimConstant.AccessTokenExpiration, new DateTimeOffset(accessTokenExpiresAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            if (workProfileId is not null)
                claims.Add(new(CustomClaimConstant.WorkProfileId, workProfileId.Value.ToString(), ClaimValueTypes.Integer64));
            if (roleId is not null)
                claims.Add(new(CustomClaimConstant.RoleId, roleId.Value.ToString(), ClaimValueTypes.Integer64));
            if (campusId is not null)
                claims.Add(new(CustomClaimConstant.CampusId, campusId.Value.ToString(), ClaimValueTypes.Integer64));
            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Subject = new ClaimsIdentity(claims),
                NotBefore = utcNow,
                Expires = accessTokenExpiresAt,
                SigningCredentials = _signingCredentials
            };
            var jsonWebTokenHandler = new JsonWebTokenHandler();
            try
            {
                var accessToken = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
                var accessTokenExpiresIn = (int)(accessTokenExpiresAt - utcNow).TotalSeconds;
                _logger.LogDebug("Generated JWT: UserId={UserId}, AccessTokenType={accessTokenType}.", userId, accessTokenType);
                return new AccessTokenDTO(accessToken, accessTokenExpiresIn, accessTokenExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT for UserId={UserId}", userId);
                throw new InfrastructureException("No se pudo generar el token de acceso.", ex);
            }
        }

        public RefreshTokenDTO GenerateRefreshToken(DateTime utcNow, int lifetimeDays)
        {
            var tokenIdentifier = Guid.NewGuid().ToString("N");
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshTokenPlain = WebEncoders.Base64UrlEncode(randomBytes);
            var refreshTokenExpiresAt = utcNow.AddDays(lifetimeDays);
            var refreshTokenExpiresIn = (int)(refreshTokenExpiresAt - utcNow).TotalSeconds;
            return new RefreshTokenDTO(tokenIdentifier, refreshTokenPlain, refreshTokenExpiresIn, refreshTokenExpiresAt);
        }
    }
}