using Business;
using PresentationBusiness;

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
    }
}