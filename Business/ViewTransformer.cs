using BusinessPersistence;
using PresentationBusiness;

namespace Business
{
    /// <summary>
    /// This is used by the View Model
    /// </summary>
    public class ViewTransformer
    {
        public ViewModelDTO toViewDTO(DepartmentDTO dto)
        {
            ViewModelDTO vdto = new ViewModelDTO();
            vdto.Name = dto.Name;
            return vdto;
        }

        public ViewModelDepartmentListDTO toListViewModelDTO(DepartmentDTO dto)
        {
            ViewModelDepartmentListDTO vdto = new ViewModelDepartmentListDTO();
            vdto.Name = dto.Name;
            return vdto;
        }

        public DepartmentDTO toDTO(ViewModelSaveDTO save_dto)
        {
            DepartmentDTO dto = new DepartmentDTO();
            dto.Name = save_dto.Name;
            return dto;
        }
    }
}
