using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistenceEF
{
    public class MyDbContext : DbContext
    {
        public IDbSet<tb_Department> MyDepartmentSet { get; set; }
        public IDbSet<tb_Department_Comment> MyCommentSet { get; set; }

        //public MyDbContext() : base(ConfigurationManager.ConnectionStrings["DB"].ConnectionString)
        public MyDbContext() : base("name=DB")
        {
            //this.Configuration.LazyLoadingEnabled = false;            
        }

        public MyDbContext(string connection_string) : base(connection_string)
        {
            //this.Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //this.Configuration.LazyLoadingEnabled = false;
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Configurations.Add(new MyTableConfiguration());
        }

        public void FixEfProviderServicesProblem()
        {
            //The Entity Framework provider type 'System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer'
            //for the 'System.Data.SqlClient' ADO.NET provider could not be loaded. 
            //Make sure the provider assembly is available to the running application. 
            //See http://go.microsoft.com/fwlink/?LinkId=260882 for more information.

            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
