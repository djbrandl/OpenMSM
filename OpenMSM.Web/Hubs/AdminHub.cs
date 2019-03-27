using Microsoft.AspNetCore.SignalR;
using OpenMSM.Data;

namespace OpenMSM.Web.Hubs
{
    public class AdminHub : Hub
    {
        public static string ActionOccurred = "ActionOccurred";
        private AppDbContext appDbContext { get; }
        public AdminHub(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
    }
}
