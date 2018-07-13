using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersistenceEF
{

    public class tb_Department
    {
        [Key]
        [Column("Department_SID")]
        public int? Sid { get; set; }
        [Column("Department")]
        public string Department { get; set; }
        [Column("Is_IT")]
        public bool IsIT { get; set; }
        public bool Active { get; set; }
        public ICollection<tb_Department_Comment> Comments { get; set; }
        public tb_Department()
        {
            Comments = new List<tb_Department_Comment>();
        }
    }
    
}
