using System.Collections.Generic;

namespace BusinessContracts
{
    public interface IDepartmentService
    {
        void Save(VMDepartmentSave dto);
        VMDepartment Get();
        IEnumerable<VMDepartmentList> GetAllActiveDepartments();
    }
}