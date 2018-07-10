using System.Collections.Generic;

namespace PersistenceContracts
{
    public interface IDepartmentDAO
    {
        void Save(DepartmentDTO dto);
        DepartmentDTO Get();
        IEnumerable<DepartmentDTO> GetAllActiveDepartments();
    }
}
