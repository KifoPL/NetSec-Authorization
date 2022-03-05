using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Identity;

namespace NetSec.MVC.Data;

public class Seed
{
    public class Roles : ISeed<Roles>
    {
        public const string Admin = "admin";
        public const string Manager = "manager";
        public const string Employee = "employee";
        public const string Default = "default";
    }

    public class Users : ISeed<Users>
    {
        private const string Password = "netsec_2022";
        public const string JKowalski = "jkowalski";
        public const string ANowak = "anowak";
        public const string Test = "test";

        public IEnumerable<(string, string)> UserPassPairs() { return this.AllValues().Select(x => (x, Password)); }
    }

    public class Permissions : ISeed<Permissions>
    {
        public const string SecretsRead = "secrets.read";
        public const string ArchiveRead = "archives.read";
        public const string ArchiveExport = "archives.export";
        public const string LobbyAccess = "lobby.access";
    }

    public class RolePermissions : ISeed<RolePermissions>
    {
        public static (string, IEnumerable<string>) AdminPerms = (Roles.Admin, new Permissions().AllValues());

        public static (string, IEnumerable<string>) ManagerPerms = (Roles.Manager, new[]
        {
            Permissions.ArchiveExport, Permissions.ArchiveRead, Permissions.LobbyAccess
        });

        public static (string, IEnumerable<string>) EmployeePerms = (Roles.Employee, new[]
        {
            Permissions.ArchiveRead, Permissions.LobbyAccess
        });

        public (string, IEnumerable<string>)[] PermRolePairs()
        {
            return new[] { AdminPerms, ManagerPerms, EmployeePerms };
        }
    }

    public class UserRoles : ISeed<UserRoles>
    {
        public static (string, string) JKowalski = (Users.JKowalski, Roles.Admin);
        public static (string, string) ANowak = (Users.ANowak, Roles.Employee);

        public (string, string)[] UserRolePairs() { return new[] { JKowalski, ANowak }; }
    }

    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IdentityResult> SeedData()
    {
        var users = new Users().UserPassPairs();

        foreach (var user in users)
            if (await _userManager.FindByNameAsync(user.Item1) is null)
            {
                var result = await _userManager.CreateAsync(new(user.Item1), user.Item2);

                if (!result.Succeeded)
                    return result;
            }


        var Roles = new Roles().AllValues();

        foreach (string role in Roles)
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new(role));

        var RolePerms = new RolePermissions().PermRolePairs();

        foreach ((string? role, var perms) in RolePerms)
            if (await _roleManager.FindByNameAsync(role) is IdentityRole foundRole)
            {
                var claims = await _roleManager.GetClaimsAsync(foundRole);
                claims = claims.Where(x => x.Type == "perm").ToList();
                foreach (string perm in perms)
                    if (!claims.Any(x => x.Value == perm))
                        await _roleManager.AddClaimAsync(foundRole, new("perm", perm));
            }

        var UserRoles = new UserRoles().UserRolePairs();

        foreach ((string? user, string? role) in UserRoles)
        {
            var idUser = await _userManager.FindByNameAsync(user);
            if (!await _userManager.IsInRoleAsync(idUser, role))
            {
                await _userManager.AddToRoleAsync(idUser, role);
            }
        }

        return IdentityResult.Success;
    }
}

public interface ISeed<T>
{
}

public static class SeedExtensions
{
    public static IEnumerable<string> AllValues<T>(this ISeed<T> seedData)
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields.Where(x => x.IsLiteral)) yield return (string)field.GetRawConstantValue()!;
    }
}