using BusinessContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation
{
    class Program
    {
        static void Main(string[] args)
        {
            Facade tf = new Facade();
            VMDepartmentSave saveDTO = new VMDepartmentSave();
            saveDTO.Name = "Test"+ DateTime.Now.Ticks;
            tf.Save(saveDTO);

            IEnumerable<VMDepartmentList> allActiveDepts = tf.GetAllActiveDepartments();
            int x = allActiveDepts.Count();            
        }
    }
}
