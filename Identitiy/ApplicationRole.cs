using Microsoft.AspNetCore.Identity;

namespace GorevNet.Identitiy
{
    public class ApplicationRole : IdentityRole
    {
        // Parametreli constructor ekleyelim
        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName) : base(roleName) { }

        // İstersen extra alanlar ekleyebilirsin
        // public string Description { get; set; }
    }
}
