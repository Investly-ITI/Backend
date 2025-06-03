using Investly.DAL.Entities;
using Investly.DAL.Repos.IRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investly.DAL.Repos
{
    public class UnitOfWork:IUnitOfWork
    {
        private readonly AppDbContext _db;
        private IInvestorRepo _InvestorRepo;
        private IUserRepo _UserRepo;
        private InvestorContactRequestRepo _InvestorContactRequestRepo;

        private IGovernmentRepo _GovernmentRepo;
        private ICityRepo _CityRepo;
        private IFounderRepo _FounderRepo;
        public IInvestorRepo InvestorRepo => _InvestorRepo ??= new InvestorRepo(_db);
        public IUserRepo UserRepo => _UserRepo ??= new UserRepo(_db);
        public IGovernmentRepo GovernmentRepo => _GovernmentRepo ??= new GovermentRepo(_db);
        public ICityRepo CityRepo => _CityRepo ??= new CityRepo(_db);
        public IFounderRepo FounderRepo => _FounderRepo ??= new FounderRepo(_db);

        public IInvestorContactRequestRepo InvestorContactRequestRepo => _InvestorContactRequestRepo ??= new InvestorContactRequestRepo(_db);

        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            
        }
        public int Save()
        {
          int res=  _db.SaveChanges();
          return res;
        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }
}
