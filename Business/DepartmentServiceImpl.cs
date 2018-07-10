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
        public VMDepartment Get()
        {
            return new VMDepartment() { Name = "test" };
        }
        public VMDepartment GetData()
        {
            DepartmentDTO dto = dao.Get();
            return viewTransformer.toViewDTO(dto);
        }

        public IEnumerable<VMDepartmentList> GetAllActiveDepartments()
        {
            IEnumerable<DepartmentDTO> listDTO = dao.GetAllActiveDepartments();
            return listDTO.Select(p => viewTransformer.toListViewModelDTO(p));
        }
        public void Save(VMDepartmentSave dto)
        {
            DepartmentDTO testdto = viewTransformer.toDTO(dto);
            dao.Save(testdto);
        }
    }
}