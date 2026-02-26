using ESAM.GrowTracking.Application.Interfaces.Infrastructure.Services;
using ESAM.GrowTracking.Infrastructure.Commons.Settings;
using ESAM.GrowTracking.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ESAM.GrowTracking.API.Commons.Extensions
{
    public static class CookieServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthCookieServices(this IServiceCollection services, IConfigurationSection cookieSettingsSection, IWebHostEnvironment env, ILogger logger, IConfiguration? configuration = null)
        {
            services.AddOptions<CookieSettings>().Bind(cookieSettingsSection).Validate(settings =>
            {
                if (settings is null)
                    return false;
                if (settings.UseHostPrefix)
                {
                    if (!string.IsNullOrWhiteSpace(settings.Domain))
                        return false;
                    var effectiveName = settings.EffectiveRefreshCookieName();
                    if (!effectiveName.StartsWith("__Host-", StringComparison.Ordinal))
                        return false;
                }
                if (settings.SameSite == SameSiteMode.None && !settings.AlwaysSecure && !settings.UseHostPrefix)
                    if (string.IsNullOrWhiteSpace(settings.XsrfCookieName))
                        return false;
                if (string.IsNullOrWhiteSpace(settings.CookieName))
                    return false;
                if (settings.XsrfCookieExpiresMinutes <= 0)
                    return false;
                return true;
            }, "CookieSettings inválida: verifique CookieName, XsrfCookieExpiresMinutes, y que UseHostPrefix=true no coexista con Domain configurado. " +
            "El nombre efectivo del cookie debe comenzar con '__Host-' cuando UseHostPrefix=true.").ValidateOnStart();
            if (env.IsProduction())
            {
                var rawSettings = cookieSettingsSection.Get<CookieSettings>();
                if (rawSettings is not null && !rawSettings.UseHostPrefix)
                    logger.LogWarning("CookieSettings: UseHostPrefix=false en producción. Se recomienda UseHostPrefix=true (__Host- prefix) para máxima seguridad de cookies. " +
                        "El prefijo __Host- garantiza Secure, Path=/ y ausencia de Domain.");
            }
            var isDistributed = configuration?.GetValue<bool>("DataProtection:IsDistributed", false) ?? false;
            if (isDistributed && !env.IsDevelopment())
                throw new InvalidOperationException("DataProtection: 'DataProtection:IsDistributed = true' detectado en entorno no-Development con keyring local. " +
                    "En entornos multi-instancia (clúster, contenedores escalados, blue-green) el IXmlRepository debe ser " +
                    "compartido entre todos los nodos (p.ej. Azure Blob Storage + Azure Key Vault, AWS SSM Parameter Store, " +
                    "base de datos centralizada). Las cookies/tokens protegidos con un keyring local serán indesencriptables " +
                    "en nodos distintos al que los generó. Configure un proveedor centralizado de claves y establezca 'DataProtection:IsDistributed = false' sólo cuando esté seguro de operar en instancia única.");
            var dpBuilder = services.AddDataProtection().SetApplicationName("ESAM.GrowTracking");
            if (!env.IsDevelopment())
            {
                var keysFolder = Path.Combine(env.ContentRootPath, "DataProtection-Keys");
                try
                {
                    Directory.CreateDirectory(keysFolder);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"DataProtection: no se pudo crear el directorio de claves '{keysFolder}'. En producción se requiere almacenamiento persistente para que el keyring " +
                        "sobreviva reinicios. Configurar una ruta accesible o un proveedor centralizado (Azure Key Vault, AWS SSM, etc.).", ex);
                }
                try
                {
                    RestrictKeysDirectoryPermissions(keysFolder);
                    logger.LogInformation("DataProtection: permisos restringidos aplicados en '{KeysFolder}'.", keysFolder);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "DataProtection: no se pudieron aplicar permisos restrictivos en '{KeysFolder}'. Revisar y restringir manualmente el directorio (chmod 700 en Unix / ACL de propietario en Windows) " +
                        "para evitar lectura de claves por otros procesos.", keysFolder);
                }
                dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
                logger.LogInformation("DataProtection: claves persistidas en '{KeysFolder}'. ADVERTENCIA: keyring local no apto para entornos multi-instancia. " +
                    "Si la API se despliega en más de una instancia/réplica, establezca 'DataProtection:IsDistributed = true' y configure un IXmlRepository centralizado antes del despliegue.", keysFolder);
            }
            else
                logger.LogWarning("DataProtection: usando keyring efímero en memoria (Development). Los tokens de refresh serán inválidos tras reiniciar el proceso.");
            services.AddScoped<IAuthCookieService, AuthCookieService>();
            return services;
        }

        private static void RestrictKeysDirectoryPermissions(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var dirInfo = new DirectoryInfo(path);
                var security = dirInfo.GetAccessControl();
                security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
                var currentUser = WindowsIdentity.GetCurrent().Name;
                security.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                dirInfo.SetAccessControl(security);
            }
        }
    }
}