namespace BusinessPersistence
{
    public interface ITestDAO
    {
        void Save(TestDTO dto);
        TestDTO Get();
    }
}
