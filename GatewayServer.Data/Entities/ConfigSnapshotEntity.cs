using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    /// <summary>
    /// 不可变的配置快照：控制面发布时编译规范化配置写入一行，数据面只读 active 行。
    /// 追加式，永不改写;active 行唯一(部分唯一索引,见 GatewayDbContext)。
    /// </summary>
    [Table("config_snapshot")]
    public class ConfigSnapshotEntity
    {
        /// <summary>
        /// 单调递增的版本号(由发布流程赋值 = 当前最大版本 + 1),数据面据此比对是否落后。
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("version")]
        public long Version { get; set; }

        /// <summary>
        /// 编译后的配置文档(JSON,形如 ConfigSnapshotDoc)。
        /// </summary>
        [Column("doc", TypeName = "jsonb")]
        public string Doc { get; set; } = "";

        /// <summary>
        /// 文档结构版本,供反序列化器独立演进。
        /// </summary>
        [Column("schema_version")]
        public int SchemaVersion { get; set; }

        /// <summary>
        /// 是否为当前生效版本。
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("published_at")]
        public long PublishedAt { get; set; }

        [Column("published_by")]
        public string PublishedBy { get; set; } = "";
    }
}
