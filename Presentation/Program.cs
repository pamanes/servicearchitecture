using BusinessContracts;
using System.Collections.Generic;
using System.Linq;

namespace Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            Facade tf = new Facade();
            ViewModelDepartmentSaveDTO saveDTO = new ViewModelDepartmentSaveDTO();
            saveDTO.Name = "Test";
            tf.Save(saveDTO);

            IEnumerable<ViewModelDepartmentListDTO> allActiveDepts = tf.GetAllActiveDepartments();
            int x = allActiveDepts.Count();
            
        }
    }
}
