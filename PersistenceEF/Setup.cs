using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistenceEF
{    
    public class Setup : EntityTypeConfiguration<tb_Department>
    {
        public Setup()
        {
            /*ToTable("dbo.tb_Department").
            HasKey(x => x.DepartmentSid);
            Property(x => x.DepartmentName).HasColumnName("Department").IsRequired();*/
        }
    }
}
