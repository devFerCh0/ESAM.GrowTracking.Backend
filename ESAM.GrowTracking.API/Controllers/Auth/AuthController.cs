using AutoMapper;
using ESAM.GrowTracking.API.Commons.Exceptions;
using ESAM.GrowTracking.API.Commons.Extensions;
using ESAM.GrowTracking.API.Commons.Mappers;
using ESAM.GrowTracking.API.Controllers.Auth.Login;
using ESAM.GrowTracking.API.Controllers.Auth.Login.Responses;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses;
using ESAM.GrowTracking.API.Controllers.Auth.LoginUserRoleCampuses;
using ESAM.GrowTracking.API.Controllers.Auth.Logout;
using ESAM.GrowTracking.API.Controllers.Auth.Refresh;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses;
using ESAM.GrowTracking.Application.Features.Auth.Logout;
using ESAM.GrowTracking.Application.Features.Auth.Refresh;
using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace ESAM.GrowTracking.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;
        private readonly IErrorToHttpMapper _errorToHttpMapper;
        private readonly IMapper _mapper;
        private readonly IAuthCookieService _authCookieService;

        public AuthController(ILogger<AuthController> logger, IMediator mediator, IErrorToHttpMapper errorToHttpMapper, IMapper mapper, IAuthCookieService authCookieService)
        {
            Guard.AgainstNull(logger, $"{nameof(logger)} no puedo ser nulo.");
            Guard.AgainstNull(mediator, $"{nameof(mediator)} no puedo ser nulo.");
            Guard.AgainstNull(errorToHttpMapper, $"{nameof(errorToHttpMapper)} no puedo ser nulo.");
            Guard.AgainstNull(mapper, $"{nameof(mapper)} no puede ser nulo.");
            Guard.AgainstNull(authCookieService, $"{nameof(authCookieService)} no puede ser nulo.");
            _logger = logger;
            _mediator = mediator;
            _errorToHttpMapper = errorToHttpMapper;
            _mapper = mapper;
            _authCookieService = authCookieService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginCommand>(request);
            var loginResult = await _mediator.Send(command, cancellationToken);
            if (!loginResult.IsSuccess)
                return loginResult.ToErrorActionResult(_errorToHttpMapper);
            var login = _mapper.Map<LoginResponse>(loginResult.Value);
            return Ok(new { success = true, data = login });
        }

        [AllowAnonymous]
        [HttpGet("user-workProfiles/workProfile/{workProfileId:int}/login-user-role-campuses")]
        [ProducesResponseType(typeof(List<LoginUserRoleCampusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<LoginUserRoleCampusResponse>>> LoginUserRoleCampusesAsync([FromRoute] int workProfileId, CancellationToken cancellationToken)
        {
            var query = new LoginUserRoleCampusQuery(workProfileId);
            var userRoleCampusesResult = await _mediator.Send(query, cancellationToken);
            if (!userRoleCampusesResult.IsSuccess)
                return userRoleCampusesResult.ToErrorActionResult(_errorToHttpMapper);
            var userRoleCampuses = _mapper.Map<List<LoginUserRoleCampusResponse>>(userRoleCampusesResult.Value);
            return Ok(new { success = true, data = userRoleCampuses });
        }

        [AllowAnonymous]
        [HttpPost("login-assumed-role")]
        [ProducesResponseType(typeof(LoginAssumedRoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginAssumedRoleResponse>> LoginAssumedRoleAsync([FromBody] LoginAssumedRoleRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginAssumedRoleCommand>(request);
            var loginAssumedRoleResult = await _mediator.Send(command, cancellationToken);
            if (!loginAssumedRoleResult.IsSuccess)
                return loginAssumedRoleResult.ToErrorActionResult(_errorToHttpMapper);
            var loginAssumedRole = loginAssumedRoleResult.Value;
            var treatAsBrowser = DetermineBrowserTreatment();
            if (treatAsBrowser)
                SetAuthenticationCookies(loginAssumedRole.RefreshTokenRaw, loginAssumedRole.RefreshTokenExpiresAt);
            return Ok(new { success = true, data = _mapper.Map<LoginAssumedRoleResponse>(loginAssumedRole, opt => opt.Items["IsBrowser"] = treatAsBrowser) });
        }

        [AllowAnonymous]
        [HttpPost("login-assumed-workProfile")]
        [ProducesResponseType(typeof(LoginAssumedWorkProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoginAssumedWorkProfileResponse>> LoginAssumedWorkProfileAsync([FromBody] LoginAssumedWorkProfileRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LoginAssumedWorkProfileCommand>(request);
            var loginAssumedWorkProfileResult = await _mediator.Send(command, cancellationToken);
            if (!loginAssumedWorkProfileResult.IsSuccess)
                return loginAssumedWorkProfileResult.ToErrorActionResult(_errorToHttpMapper);
            var loginAssumedWorkProfile = loginAssumedWorkProfileResult.Value;
            var treatAsBrowser = DetermineBrowserTreatment();
            if (treatAsBrowser)
                SetAuthenticationCookies(loginAssumedWorkProfile.RefreshTokenRaw, loginAssumedWorkProfile.RefreshTokenExpiresAt);
            return Ok(new { success = true, data = _mapper.Map<LoginAssumedWorkProfileResponse>(loginAssumedWorkProfile, opt => opt.Items["IsBrowser"] = treatAsBrowser) });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<LogoutCommand>(request);
            bool hadIncomingCookieToken = false;
            if (string.IsNullOrWhiteSpace(command.RefreshTokenRaw))
                if (_authCookieService.TryGetRefreshTokenFromRequest(Request, out var cookieToken))
                {
                    command = command with { RefreshTokenRaw = cookieToken };
                    hadIncomingCookieToken = true;
                }
            var logoutResult = await _mediator.Send(command, cancellationToken);
            if (!logoutResult.IsSuccess)
                return logoutResult.ToErrorActionResult(_errorToHttpMapper);
            if (hadIncomingCookieToken)
                DeleteAuthenticationCookies();
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            var command = _mapper.Map<RefreshCommand>(request);
            bool hadIncomingCookieToken = false;
            if (string.IsNullOrWhiteSpace(command.RefreshTokenRaw))
                if (_authCookieService.TryGetRefreshTokenFromRequest(Request, out var cookieToken))
                {
                    command = command with { RefreshTokenRaw = cookieToken };
                    hadIncomingCookieToken = true;
                }
            var refreshResult = await _mediator.Send(command, cancellationToken);
            if (!refreshResult.IsSuccess)
                return refreshResult.ToErrorActionResult(_errorToHttpMapper);
            var refresh = refreshResult.Value;
            bool treatAsBrowser = IsLikelyBrowserRequest(Request) || hadIncomingCookieToken;
            if (treatAsBrowser)
                SetAuthenticationCookies(refresh.RefreshTokenRaw, refresh.RefreshTokenExpiresAt);
            return Ok(new { success = true, data = _mapper.Map<RefreshResponse>(refresh, opt => opt.Items["IsBrowser"] = treatAsBrowser) });;
        }

        private bool DetermineBrowserTreatment()
        {
            bool hadIncomingCookieToken = _authCookieService.TryGetRefreshTokenFromRequest(Request, out _);
            return IsLikelyBrowserRequest(Request) || hadIncomingCookieToken;
        }

        private void SetAuthenticationCookies(string? refreshTokenRaw, DateTime refreshTokenExpiresAt)
        {
            if (!string.IsNullOrWhiteSpace(refreshTokenRaw))
            {
                try
                {
                    var expiresAt = new DateTimeOffset(refreshTokenExpiresAt.ToUniversalTime());
                    _authCookieService.SetRefreshTokenCookie(Response, refreshTokenRaw, expiresAt);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to set refresh token cookie.");
                }
            }
            try
            {
                var xsrfToken = GenerateXsrfToken();
                _authCookieService.SetXsrfCookie(Response, xsrfToken);
                if (!Response.Headers.ContainsKey("X-XSRF-TOKEN"))
                    Response.Headers["X-XSRF-TOKEN"] = xsrfToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set XSRF cookie/header.");
            }
        }

        private void DeleteAuthenticationCookies()
        {
            try
            {
                _authCookieService.DeleteRefreshTokenCookie(Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete refresh token cookie during logout.");
            }
            try
            {
                _authCookieService.DeleteXsrfCookie(Response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete XSRF cookie during logout.");
            }
        }

        private static bool IsLikelyBrowserRequest(HttpRequest request)
        {
            if (request is null)
                return false;
            if (request.Headers.ContainsKey("Origin"))
                return true;
            if (request.Headers.ContainsKey("Referer"))
                return true;
            if (request.Headers.TryGetValue("User-Agent", out var ua))
            {
                var uaStr = ua.ToString();
                if ((uaStr.Contains("Mozilla", StringComparison.OrdinalIgnoreCase) || uaStr.Contains("Chrome", StringComparison.OrdinalIgnoreCase) || uaStr.Contains("Safari", StringComparison.OrdinalIgnoreCase)) 
                    && !uaStr.Contains("Postman", StringComparison.OrdinalIgnoreCase) && !uaStr.Contains("curl", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string GenerateXsrfToken()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}