using BusinessPersistence;
using PresentationBusiness;

namespace Business
{
    /// <summary>
    /// This is used by the View Model
    /// </summary>
    public class ViewTransformer
    {
        public ViewModelDTO toViewDTO(TestDTO dto)
        {
            ViewModelDTO vdto = new ViewModelDTO();
            vdto.Name = dto.Name;
            return vdto;
        }

        public TestDTO toDTO(ViewModelSaveDTO save_dto)
        {
            TestDTO dto = new TestDTO();
            dto.Name = save_dto.Name;
            return dto;
        }
    }
}
