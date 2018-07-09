using Business;
using PresentationBusiness;
using System.Collections;
using System.Collections.Generic;

namespace Presentation
{
    public class TestFacade
    {
        readonly ITestService testService;
        public TestFacade()
        {
            testService = new TestServiceImpl();
        }
        public ViewModelDTO Get()
        {
            ViewModelDTO vmdto = testService.Get();
            return vmdto;
        }
        public void Save(ViewModelSaveDTO dto)
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