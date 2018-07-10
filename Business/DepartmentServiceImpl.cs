using PersistenceContracts;
using PersistenceEF;
//using Persistence;
using BusinessContracts;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    public class DepartmentServiceImpl : IDepartmentService
    {
        ViewTransformer viewTransformer = new ViewTransformer();
        IDepartmentDAO dao;
        public DepartmentServiceImpl()
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
        public void Save(ViewModelDepartmentSaveDTO dto)
        {
            DepartmentDTO testdto = viewTransformer.toDTO(dto);
            dao.Save(testdto);
        }
    }
}
