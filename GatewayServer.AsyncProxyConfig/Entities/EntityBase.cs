using SqlSugar;

namespace GatewayServer.AsyncProxyConfig.Entities
{
    public class EntityBase
    {

        [SugarColumn(ColumnName = "modifier_name")]
        public string ModifierName { get; set; } = "";


        [SugarColumn(ColumnName = "modify_date")]
        public long ModifyDate { get; set; }

        [SugarColumn(ColumnName = "creator_name")]
        public string CreatorName { get; set; } = "";

        [SugarColumn(ColumnName = "create_date")]
        public long CreateDate { get; set; }
    }
}
