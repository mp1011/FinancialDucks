using FinancialDucks.Application.Models;
using System;
using System.Collections.Generic;

namespace FinancialDucks.Infrastructure.Models
{
    public partial class CategoryRule : ICategoryRuleDetail
    {
        ICategory ICategoryRuleDetail.Category => Category;
    }
}
