using BusinessPersistence;
using System;

namespace Persistence
{
    public class TestDAOImpl : ITestDAO
    {
        DTOTransformer transformer = new DTOTransformer();
        public void Save(TestDTO dto)
        {
            TestEntity te = transformer.ToEntity(dto);
            Console.Out.WriteLine("save was called");
        }

        public TestDTO Get()
        {
            return new TestDTO() { Name = "test", Sid = 10 } ;
        }
    }
}
