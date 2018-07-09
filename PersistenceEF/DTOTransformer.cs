using BusinessPersistence;

namespace PersistenceEF
{
    public class DTOTransformer
    {
        public tb_Department ToEntity(DepartmentDTO dto)
        {
            tb_Department te = new tb_Department();
            te.DepartmentSid = dto.Sid;
            te.DepartmentName = dto.Name;
            return te;
        }

        public DepartmentDTO ToDTO(tb_Department te)
        {
            DepartmentDTO tdto = new DepartmentDTO();
            tdto.Sid = te.DepartmentSid;
            tdto.Name = te.DepartmentName;
            return tdto;
        }
    }
}
