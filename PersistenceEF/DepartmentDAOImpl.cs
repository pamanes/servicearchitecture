using PersistenceContracts;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Configuration;
using System.Data.Entity.Migrations;
//var stuff = context.Database.SqlQuery<Something>("select 1 as [one], 2 as [two] from dbo.tb_Department;");
namespace PersistenceEF
{
    public class DepartmentDAOImpl : IDepartmentDAO
    {
        DTOTransformer transformer = new DTOTransformer();
        public void Save(DepartmentDTO dto)
        {
            //using (var dbContextTransaction = context.Database.BeginTransaction()) 
            //context.SaveChanges(); 
            //dbContextTransaction.Commit();
            //dbContextTransaction.Rollback();
            //http://codethug.com/2016/02/19/Entity-Framework-Cache-Busting/
            tb_Department te = transformer.ToEntity(dto);
            tb_Department t;
            using (MyDbContext context = new MyDbContext())
            {
                //var stuff = context.MyDepartmentSet.Include(E => E.Comments.FirstOrDefault()).ToList();
                //var stuff = context.MyDepartmentSet.Include(d => d.Comments).Select(d => new { Department = d, HasChild = d.Comments.FirstOrDefault() }).ToList();

                var stuff = context.MyDepartmentSet.Include(d => d.Comments).Select(d => new { Department = d, HasChild = d.Comments.Any() }).ToList();
                context.MyDepartmentSet.AddOrUpdate(p => new { p.Department }, te);
                //context.MyDepartmentSet.Add(te);
                context.SaveChanges();
                
                tb_Department_Comment c = new tb_Department_Comment() { Comment = te.Department };
                te.Comments.Add(c);
                
                context.SaveChanges();
                
                t = context.MyDepartmentSet
                    .Where(d => d.Sid.Value == 1876)
                    .AsNoTracking()
                    .FirstOrDefault();
                /*
                tb_Department_Comment newC = new tb_Department_Comment() { Department_SID = te.Sid.Value, Comment = "hi" };
                context.MyCommentSet.Add(newC);
                context.SaveChanges();
                */
                var loadedComment = context.MyCommentSet.Where(cc => cc.Sid.Value == 1).SingleOrDefault();
                var loadedComment2 = context.MyCommentSet.Where(cc => cc.Sid.Value == 2).SingleOrDefault();

            }
            Console.Out.WriteLine("save was called");
        }

        IEnumerable<tb_Department> GetList()
        {
            IEnumerable<tb_Department> departments;
            using (MyDbContext context = new MyDbContext())
            {
                departments = context.MyDepartmentSet.Where(d => d.Active == true).ToList();

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
