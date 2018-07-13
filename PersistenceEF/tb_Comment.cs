using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersistenceEF
{
    public class tb_Department_Comment
    {
        [Key]
        [Column("Comment_SID")]
        public int? Sid { get; set; }
        [ForeignKey("Department")]
        public int Department_SID { get; set; }        
        public tb_Department Department { get; set; }
        public string Comment { get; set; }
    }
}