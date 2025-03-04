namespace Marelli.Business.Utils;

public class UserUtils
{
    public static bool VerifyIsMarelliUser(string userRole)
    {
        var marelliUsers = new[]
        {
            "Administrator", "Project Manager", "Software Manager", "Software Project Leader", "Domain Leader",
            "Integrator", "Developer"
        };

        return marelliUsers.Contains(userRole);
    }
}