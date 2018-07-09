using System.Collections.Generic;

namespace BusinessPersistence
{
    public interface IDepartmentDAO
    {
        void Save(DepartmentDTO dto);
        DepartmentDTO Get();
        IEnumerable<DepartmentDTO> GetAllActiveDepartments();
    }
}
