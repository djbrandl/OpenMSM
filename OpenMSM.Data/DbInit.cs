using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMSM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;

namespace OpenMSM.Data
{
    public static class DbInit
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
