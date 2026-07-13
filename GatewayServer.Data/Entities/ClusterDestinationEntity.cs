using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    [Table("destination")]
    public class ClusterDestinationEntity : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        /// <summary>
        /// 关联的集群
        /// </summary>
        [Column("cluster_code")]
        public string ClusterCode { get; set; } = "";

        /// <summary>
        /// 匹配地址
        /// </summary>
        [Column("address")]
        public string Address { get; set; } = "";

        /// <summary>
        /// 健康检查地址
        /// </summary>
        [Column("health_check_path")]
        public string HealthCheckPath { get; set; } = "";

        [Column("is_deleted")]
        public short IsDeleted { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";
    }
}
