using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investly.DAL.Repos.IRepos
{
   public interface IUnitOfWork
    {
        public IInvestorRepo InvestorRepo { get; }
        public IUserRepo UserRepo { get; }
        public IGovernmentRepo GovernmentRepo { get; }
        public ICityRepo CityRepo { get; }
        public IFounderRepo FounderRepo { get; }
        public int Save();
        public void Dispose();

        public IInvestorContactRequestRepo InvestorContactRequestRepo { get; }


    }
}
