using PersistenceContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
//var stuff = context.Database.SqlQuery<Something>("select 1 as [one], 2 as [two] from dbo.tb_Department;");
namespace PersistenceEF
{
    public class DepartmentDAOImpl : IDepartmentDAO
    {
        DTOTransformer transformer = new DTOTransformer();
        public void Save(DepartmentDTO dto)
        {
            tb_Department te = transformer.ToEntity(dto);
            using (MyDbContext context = new MyDbContext())
            {                
                context.MyDepartmentSet.Add(te);
                context.SaveChanges();
                
            }
            Console.Out.WriteLine("save was called");
        }

        IEnumerable<tb_Department> GetList()
        {
            IEnumerable<tb_Department> departments;
            using (MyDbContext context = new MyDbContext())
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
