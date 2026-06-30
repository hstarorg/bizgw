using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    /// <summary>
    /// 网关实例心跳/状态:每实例每隔若干秒 upsert 自己的行。控制面据此看集群存活、版本、reload 成败。
    /// </summary>
    [Table("instance_status")]
    public class InstanceStatusEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("instance_id")]
        public string InstanceId { get; set; } = "";

        [Column("hostname")]
        public string Hostname { get; set; } = "";

        /// <summary>本实例当前已应用的配置(快照)版本。</summary>
        [Column("applied_config_version")]
        public long AppliedConfigVersion { get; set; }

        [Column("last_reload_at")]
        public long LastReloadAt { get; set; }

        [Column("last_reload_ok")]
        public bool LastReloadOk { get; set; }

        [Column("started_at")]
        public long StartedAt { get; set; }

        [Column("last_heartbeat_at")]
        public long LastHeartbeatAt { get; set; }
    }
}
