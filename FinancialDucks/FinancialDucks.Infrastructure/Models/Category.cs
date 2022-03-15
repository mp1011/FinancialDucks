﻿using Microsoft.EntityFrameworkCore;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class Category
    {
        public Category()
        {
            CategoryRules = new HashSet<CategoryRule>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public HierarchyId HierarchyId { get; set; }

        public virtual ICollection<CategoryRule> CategoryRules { get; set; }
    }
}
