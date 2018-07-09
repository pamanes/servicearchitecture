using BusinessPersistence;
using Persistence;
using PresentationBusiness;

namespace Business
{
    public class TestServiceImpl : ITestService
    {
        ViewTransformer viewTransformer = new ViewTransformer();
        ITestDAO dao;
        public TestServiceImpl()
        {
            dao = new TestDAOImpl();
        }
        public ViewModelDTO Get()
        {
            return new ViewModelDTO() { Name = "test" };
        }
        public ViewModelDTO GetData()
        {
            TestDTO dto = dao.Get();
            return viewTransformer.toViewDTO(dto);
        }
        public void Save(ViewModelSaveDTO dto)
        {
            TestDTO testdto = viewTransformer.toDTO(dto);
            dao.Save(testdto);
        }
    }
}
