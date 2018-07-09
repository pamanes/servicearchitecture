using BusinessPersistence;

namespace Persistence
{
    public class DTOTransformer
    {
        public TestEntity ToEntity(TestDTO dto)
        {
            TestEntity te = new TestEntity();
            te.Sid = dto.Sid;
            te.Name = dto.Name;
            return te;
        }

        public TestDTO ToDTO(TestEntity te)
        {
            TestDTO tdto = new TestDTO();
            te.Sid = te.Sid;
            te.Name = te.Name;
            return tdto;
        }
    }
}
