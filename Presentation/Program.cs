using PresentationBusiness;
using System.Collections.Generic;
using System.Linq;

namespace Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            TestFacade tf = new TestFacade();
            ViewModelSaveDTO saveDTO = new ViewModelSaveDTO();
            saveDTO.Name = "Test";
            tf.Save(saveDTO);

            IEnumerable<ViewModelDepartmentListDTO> allActiveDepts = tf.GetAllActiveDepartments();
            int x = allActiveDepts.Count();
            
        }
    }
}
