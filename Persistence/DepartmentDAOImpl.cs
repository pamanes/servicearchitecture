using PersistenceContracts;
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
            Console.Out.WriteLine("Save was called");
        }

        public DepartmentDTO Get()
        {
            Console.Out.WriteLine("Get was called");
            return new DepartmentDTO() { Name = "test", Sid = 10 } ;
        }

        public IEnumerable<DepartmentDTO> GetAllActiveDepartments()
        {
            Console.Out.WriteLine("GetAllActiveDepartments was called");
            return new List<DepartmentDTO>()
            {
                new DepartmentDTO(){ Name = "test1", Sid= 1 },
                new DepartmentDTO(){ Name = "test2", Sid= 2 }
            };
        }
    }
}
