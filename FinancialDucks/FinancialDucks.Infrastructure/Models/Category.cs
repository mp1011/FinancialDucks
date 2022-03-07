using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public HierarchyId HierarchyId { get; set; }
    }
}
