using System.Collections;
using System.Collections.Generic;

namespace PresentationBusiness
{
    public interface ITestService
    {
        void Save(ViewModelSaveDTO dto);
        ViewModelDTO Get();
        IEnumerable<ViewModelDepartmentListDTO> GetAllActiveDepartments();
    }
}
