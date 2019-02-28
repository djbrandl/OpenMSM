using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ISBM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;

namespace ISBM.Data
{
    public static class DbInit
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
