using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    [Table("route")]
    public class RouteEntity : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("route_name")]
        public string RouteName { get; set; } = "";

        /// <summary>
        /// 要关联到的集群
        /// </summary>
        [Column("cluster_code")]
        public string ClusterCode { get; set; } = "";

        [Column("match_path")]
        public string MatchPath { get; set; } = "";

        /// <summary>
        /// 允许的 Method，竖线分割
        /// </summary>
        [Column("match_methods")]
        public string MatchMethods { get; set; } = "";


        [Column("transforms")]
        public string Transforms { get; set; } = "";

        /// <summary>
        /// 是否删除，逻辑删
        /// </summary>
        [Column("is_deleted")]
        public short IsDeleted { get; set; } = 0;

        [Column("remark")]
        public string Remark { get; set; } = "";
    }
}
