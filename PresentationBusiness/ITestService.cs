namespace PresentationBusiness
{
    public interface ITestService
    {
        void Save(ViewModelSaveDTO dto);
        ViewModelDTO Get();
    }
}
