using BusinessPersistence;
//using PersistenceEF;
using Persistence;
using PresentationBusiness;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    public class TestServiceImpl : ITestService
    {
        ViewTransformer viewTransformer = new ViewTransformer();
        IDepartmentDAO dao;
        public TestServiceImpl()
        {
            dao = new DepartmentDAOImpl();
        }
        public ViewModelDTO Get()
        {
            return new ViewModelDTO() { Name = "test" };
        }
        public ViewModelDTO GetData()
        {
            DepartmentDTO dto = dao.Get();
            return viewTransformer.toViewDTO(dto);
        }

        public IEnumerable<ViewModelDepartmentListDTO> GetAllActiveDepartments()
        {
            IEnumerable<DepartmentDTO> listDTO = dao.GetAllActiveDepartments();
            return listDTO.Select(p => viewTransformer.toListViewModelDTO(p));
        }
        public void Save(ViewModelSaveDTO dto)
        {
            DepartmentDTO testdto = viewTransformer.toDTO(dto);
            dao.Save(testdto);
        }
    }
}
