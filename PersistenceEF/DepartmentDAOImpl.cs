using PersistenceContracts;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PersistenceEF
{
    public class DepartmentDAOImpl : IDepartmentDAO
    {
        DTOTransformer transformer = new DTOTransformer();
        public void Save(DepartmentDTO dto)
        {
            tb_Department te = transformer.ToEntity(dto);
            IEnumerable<tb_Department> list = GetList();
            using (MyDbContext context = new MyDbContext(@"MultipleActiveResultSets=True;Data Source=CORALEPAL1\SQL2014; Initial Catalog=Training;Connect Timeout=300;User Id=traininguser;Password=trainingpwd;Application Name=Training;"))
            {
                te.Department += DateTime.Now.Ticks;
                context.MyDepartmentSet.Add(te);
                context.SaveChanges();
                //var stuff = context.Database.SqlQuery<Something>("select 1 as [one], 2 as [two] from dbo.tb_Department;");
            }
            Console.Out.WriteLine("save was called");
        }

        IEnumerable<tb_Department> GetList()
        {
            IEnumerable<tb_Department> departments;
            using (MyDbContext context = new MyDbContext(@"MultipleActiveResultSets=True;Data Source=CORALEPAL1\SQL2014; Initial Catalog=Training;Connect Timeout=300;User Id=traininguser;Password=trainingpwd;Application Name=Training;"))
            {
                departments = (from d in context.MyDepartmentSet
                               where d.Active == true
                               select d).ToList();

            }
            return departments;
        }
        public IEnumerable<DepartmentDTO> GetAllActiveDepartments()
        {
            IEnumerable<tb_Department> t_depts = GetList();
            return t_depts.Select(p => transformer.ToDTO(p));
        }
        public DepartmentDTO Get()
        {
            throw new NotImplementedException();
        }
    }
}
