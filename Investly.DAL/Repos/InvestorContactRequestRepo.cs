using Investly.DAL.Entities;
using Investly.DAL.Repos.IRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Investly.DAL.Repos
{
    public class InvestorContactRequestRepo : Repo<InvestorContactRequest>, IInvestorContactRequestRepo
    {
        public InvestorContactRequestRepo(AppDbContext db) : base(db)
        {
        }
    }
}
