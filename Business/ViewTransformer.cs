using PersistenceContracts;
using BusinessContracts;

namespace Business
{
    /// <summary>
    /// This is used by the View Model
    /// </summary>
    public class ViewTransformer
    {
        public VMDepartment toViewDTO(DepartmentDTO dto)
        {
            VMDepartment vdto = new VMDepartment();
            vdto.Name = dto.Name;
            return vdto;
        }
        public VMDepartmentList toListViewModelDTO(DepartmentDTO dto)
        {
            VMDepartmentList vdto = new VMDepartmentList();
            vdto.Name = dto.Name;
            return vdto;
        }
        public DepartmentDTO toDTO(VMDepartmentSave save_dto)
        {
            DepartmentDTO dto = new DepartmentDTO();
            dto.Name = save_dto.Name;
            return dto;
        }
    }
}
