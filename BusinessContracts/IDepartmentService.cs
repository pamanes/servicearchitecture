using System.Collections;
using System.Collections.Generic;

namespace BusinessContracts
{
    public interface IDepartmentService
    {
        void Save(ViewModelDepartmentSaveDTO dto);
        ViewModelDTO Get();
        IEnumerable<ViewModelDepartmentListDTO> GetAllActiveDepartments();
    }
}