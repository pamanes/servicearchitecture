using Business;
using BusinessContracts;
using System.Collections.Generic;

namespace Presentation
{
    public class Facade
    {
        readonly IDepartmentService testService;
        public Facade()
        {
            testService = new DepartmentServiceImpl();
        }
        public VMDepartment Get()
        {
            VMDepartment vmdto = testService.Get();
            return vmdto;
        }
        public void Save(VMDepartmentSave dto)
        {
            testService.Save(dto);
        }

        public IEnumerable<VMDepartmentList> GetAllActiveDepartments()
        {
            IEnumerable<VMDepartmentList> activeDepts = testService.GetAllActiveDepartments();
            return activeDepts;
        }
    }
}