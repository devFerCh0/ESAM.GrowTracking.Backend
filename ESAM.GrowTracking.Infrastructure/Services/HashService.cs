using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Exceptions;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public class HashService : IHashService, IDisposable
    {
        private readonly ILogger<HashService> _logger;
        private readonly RandomNumberGenerator _rng;
        private readonly int _defaultSaltSize;
        private readonly int _defaultIterations;
        private readonly int _defaultHashSize;
        private bool _disposed;

        public HashService(ILogger<HashService> logger, RandomNumberGenerator? rng = null, int defaultSaltSize = 16, int defaultIterations = 10000, int defaultHashSize = 32)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puede ser nulo");
            Guard.Against(defaultSaltSize <= 0, $"{nameof(defaultSaltSize)} debe ser mayor a cero");
            Guard.Against(defaultIterations <= 0, $"{nameof(defaultIterations)} debe ser mayor a cero");
            Guard.Against(defaultHashSize <= 0, $"{nameof(defaultHashSize)} debe ser mayor a cero");
            _logger = logger;
            _defaultSaltSize = defaultSaltSize;
            _defaultIterations = defaultIterations;
            _defaultHashSize = defaultHashSize;
            _rng = rng ?? RandomNumberGenerator.Create();
        }

        public string GenerateSalt(int size = 0)
        {
            Guard.Against(size < 0, $"{nameof(size)} debe ser mayor a cero");
            var actualSize = size > 0 ? size : _defaultSaltSize;
            var saltBytes = new byte[actualSize];
            _rng.GetBytes(saltBytes);
            var salt = Convert.ToBase64String(saltBytes);
            return salt;
        }

        public string ComputeHash(string input, string salt, int? iterations = null, int? hashSize = null)
        {
            Guard.AgainstNull(input, $"{nameof(input)} no puede ser nulo");
            Guard.AgainstNull(salt, $"{nameof(salt)} no puede ser nulo");
            var iterationsCount = iterations ?? _defaultIterations;
            var derivedHashSize = hashSize ?? _defaultHashSize;
            Guard.Against(iterationsCount <= 0, $"{nameof(iterationsCount)} debe ser mayor a cero");
            Guard.Against(derivedHashSize <= 0, $"{nameof(derivedHashSize)} debe ser mayor a cero");
            byte[] saltBytes;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Salt proporcionada no es Base64 válido.");
                throw new InfrastructureException("La sal proporcionada no es una cadena Base64 válida.", ex);
            }
            using var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, iterationsCount, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(derivedHashSize);
            var hash = Convert.ToBase64String(hashBytes);
            return hash;
        }

        public bool VerifyHash(string input, string salt, string expectedHash, int? iterations = null, int? hashSize = null)
        {
            Guard.AgainstNull(expectedHash, $"{nameof(expectedHash)} no puede ser nulo");
            var computedHash = ComputeHash(input, salt, iterations, hashSize);
            byte[] computedBytes = Convert.FromBase64String(computedHash);
            byte[] originalBytes;
            try
            {
                originalBytes = Convert.FromBase64String(expectedHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "El hash proporcionado no es una cadena Base64 válida.");
                throw new InfrastructureException("El hash proporcionado no es una cadena Base64 válida.", ex);
            }
            var isEqual = CryptographicOperations.FixedTimeEquals(computedBytes, originalBytes);
            return isEqual;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                _rng.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}