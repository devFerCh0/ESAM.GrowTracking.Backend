using ESAM.GrowTracking.Application.Commons.Exceptions;
using ESAM.GrowTracking.Application.Interfaces.Percistence.Repositories;
using ESAM.GrowTracking.Application.Interfaces.Services;
using ESAM.GrowTracking.Domain.Entities;

namespace ESAM.GrowTracking.Application.Services
{
    public class BlacklistedTokenService : IBlacklistedTokenService
    {
        private readonly IBlacklistedAccessTokenTemporaryRepository _blacklistedAccessTokenTemporaryRepository;
        private readonly IBlacklistedAccessTokenPermanentRepository _blacklistedAccessTokenPermanentRepository;
        private readonly IBlacklistedRefreshTokenRepository _blacklistedRefreshTokenRepository;

        public BlacklistedTokenService(IBlacklistedAccessTokenTemporaryRepository blacklistedAccessTokenTemporaryRepository, IBlacklistedAccessTokenPermanentRepository blacklistedAccessTokenPermanentRepository, 
            IBlacklistedRefreshTokenRepository blacklistedRefreshTokenRepository)
        {
            Guard.AgainstNull(blacklistedAccessTokenTemporaryRepository, $"{nameof(blacklistedAccessTokenTemporaryRepository)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedAccessTokenPermanentRepository, $"{nameof(blacklistedAccessTokenPermanentRepository)} no puede ser nulo.");
            Guard.AgainstNull(blacklistedRefreshTokenRepository, $"{nameof(blacklistedRefreshTokenRepository)} no puede ser nulo.");
            _blacklistedAccessTokenTemporaryRepository = blacklistedAccessTokenTemporaryRepository;
            _blacklistedAccessTokenPermanentRepository = blacklistedAccessTokenPermanentRepository;
            _blacklistedRefreshTokenRepository = blacklistedRefreshTokenRepository;
        }

        public async Task<BlacklistedAccessTokenTemporary?> TryGenerateBlacklistedAccessTokenTemporaryAsync(int userId, string jti, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy, 
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var existsBlacklistedAccessTokenTemporary = await _blacklistedAccessTokenTemporaryRepository.ExistsAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
            return existsBlacklistedAccessTokenTemporary ? null : new BlacklistedAccessTokenTemporary(userId: userId, jti: jti, expirationDate: expirationDate, blacklistedAt: blacklistedAt, reason: reason, 
                createdBy: createdBy);
        }

        public async Task<BlacklistedAccessTokenPermanent?> TryGenerateBlacklistedAccessTokenPermanentAsync(int userSessionId, string jti, DateTime expirationDate, DateTime blacklistedAt, string reason, int createdBy,
            bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var existsBlacklistedAccessTokenPermanent = await _blacklistedAccessTokenPermanentRepository.ExistsAsync(jti: jti, asTracking: asTracking, cancellationToken: cancellationToken);
            return existsBlacklistedAccessTokenPermanent ? null : new BlacklistedAccessTokenPermanent(userSessionId: userSessionId, jti: jti, expirationDate: expirationDate, blacklistedAt: blacklistedAt, reason: reason,
                createdBy: createdBy);
        }

        public async Task<BlacklistedRefreshToken?> TryGenerateBlacklistedRefreshTokenAsync(int userSessionRefreshTokenId, string tokenIdentifier, DateTime expirationDate, DateTime blacklistedAt, string reason, 
            int createdBy, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var existsBlacklistedRefreshToken = await _blacklistedRefreshTokenRepository.ExistsAsync(tokenIdentifier: tokenIdentifier, asTracking: asTracking, cancellationToken: cancellationToken);
            return existsBlacklistedRefreshToken ? null : new BlacklistedRefreshToken(userSessionRefreshTokenId: userSessionRefreshTokenId, tokenIdentifier: tokenIdentifier, expirationDate: expirationDate, 
                blacklistedAt: blacklistedAt, reason: reason, createdBy: createdBy);
        }

        public async Task<List<BlacklistedRefreshToken>> GetPendingBlacklistedRefreshTokensAsync(List<(int Id, string TokenIdentifier, DateTime ExpiresAt)> userSessionRefreshTokens, DateTime blacklistedAt, string reason, 
            int currentUserId, bool asTracking = false, CancellationToken cancellationToken = default)
        {
            var tokenIdentifiers = userSessionRefreshTokens.Select(ust => ust.TokenIdentifier).ToList();
            var existingTokenIdentifiers = await _blacklistedRefreshTokenRepository.GetExistingTokenIdentifiersAsync(tokenIdentifiers: tokenIdentifiers, asTracking: asTracking, cancellationToken: cancellationToken);
            var userSessionRefreshTokensForBlacklistedRefreshToken = userSessionRefreshTokens.Where(ust => !existingTokenIdentifiers.Contains(ust.TokenIdentifier)).ToList();
            return [.. userSessionRefreshTokensForBlacklistedRefreshToken.Select(usrt => new BlacklistedRefreshToken(userSessionRefreshTokenId: usrt.Id, tokenIdentifier: usrt.TokenIdentifier, expirationDate: usrt.ExpiresAt, 
                blacklistedAt: blacklistedAt, reason: reason, createdBy: currentUserId))];
        }
    }
}