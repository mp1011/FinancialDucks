using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class Category
    {
        public Category()
        {
            BudgetLines = new HashSet<BudgetLine>();
            CategoryRules = new HashSet<CategoryRule>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public HierarchyId HierarchyId { get; set; }
        public bool Starred { get; set; }

        public virtual ICollection<BudgetLine> BudgetLines { get; set; }
        public virtual ICollection<CategoryRule> CategoryRules { get; set; }
    }
}
