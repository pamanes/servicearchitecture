using BusinessPersistence;
using System;
using System.Collections.Generic;

namespace Persistence
{
    public class DepartmentDAOImpl : IDepartmentDAO
    {
        DTOTransformer transformer = new DTOTransformer();
        public void Save(DepartmentDTO dto)
        {
            TDepartment te = transformer.ToEntity(dto);
            Console.Out.WriteLine("save was called");
        }

        public DepartmentDTO Get()
        {
            return new DepartmentDTO() { Name = "test", Sid = 10 } ;
        }

        public IEnumerable<DepartmentDTO> GetAllActiveDepartments()
        {
            throw new NotImplementedException();
        }
    }
}
