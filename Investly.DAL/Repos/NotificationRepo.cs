using Investly.DAL.Entities;
using Investly.DAL.Repos.IRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Investly.DAL.Repos
{
    public class NotificationRepo : Repo<Notification>, INotificationRepo
    {
        private readonly AppDbContext _db;
        public NotificationRepo(AppDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
