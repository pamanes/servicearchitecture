using BusinessPersistence;

namespace Persistence
{
    public class DTOTransformer
    {
        public TDepartment ToEntity(DepartmentDTO dto)
        {
            TDepartment te = new TDepartment();
            te.Sid = dto.Sid;
            te.Name = dto.Name;
            return te;
        }

        public DepartmentDTO ToDTO(TDepartment te)
        {
            DepartmentDTO tdto = new DepartmentDTO();
            tdto.Sid = te.Sid;
            tdto.Name = te.Name;
            return tdto;
        }
    }
}
