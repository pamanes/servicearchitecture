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
        public ViewModelDTO Get()
        {
            ViewModelDTO vmdto = testService.Get();
            return vmdto;
        }
        public void Save(ViewModelDepartmentSaveDTO dto)
        {
            testService.Save(dto);
        }

        public IEnumerable<ViewModelDepartmentListDTO> GetAllActiveDepartments()
        {
            IEnumerable<ViewModelDepartmentListDTO> activeDepts = testService.GetAllActiveDepartments();
            return activeDepts;
        }
    }
}