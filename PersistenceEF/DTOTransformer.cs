using PersistenceContracts;

namespace PersistenceEF
{
    public class DTOTransformer
    {
        public tb_Department ToEntity(DepartmentDTO dto)
        {
            tb_Department te = new tb_Department();
            te.Sid = dto.Sid;
            te.Department = dto.Name;
            return te;
        }

        public DepartmentDTO ToDTO(tb_Department te)
        {
            DepartmentDTO tdto = new DepartmentDTO();
            tdto.Sid = te.Sid.Value;
            tdto.Name = te.Department;
            return tdto;
        }
    }
}
