using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistenceEF
{

    public class tb_Department
    {
        [Key]
        [Column("Department_SID")]
        public int DepartmentSid { get; set; }
        [Column("Department")]
        public string DepartmentName { get; set; }
        [Column("Is_IT")]
        public bool IsIT { get; set; }
        public bool Active { get; set; }
    }
    
}
