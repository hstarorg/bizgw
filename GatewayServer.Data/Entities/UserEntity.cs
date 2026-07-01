using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GatewayServer.Data.Entities
{
    /// <summary>
    /// 管理后台登录账户。role = Owner / Editor / Viewer。表名用 app_user 避开 PG 保留字 user。
    /// </summary>
    [Table("app_user")]
    public class UserEntity : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        [Column("username")]
        public string Username { get; set; } = "";

        [Column("password_hash")]
        public string PasswordHash { get; set; } = "";

        [Column("role")]
        public string Role { get; set; } = "";

        [Column("is_deleted")]
        public short IsDeleted { get; set; }
    }
}
