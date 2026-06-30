using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    public class EntityBase
    {
        [Column("modifier_name")]
        public string ModifierName { get; set; } = "";

        [Column("modify_date")]
        public long ModifyDate { get; set; }

        [Column("creator_name")]
        public string CreatorName { get; set; } = "";

        [Column("create_date")]
        public long CreateDate { get; set; }
    }
}
