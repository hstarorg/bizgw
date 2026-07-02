using System.ComponentModel.DataAnnotations;

namespace GatewayServer.ControlPlane.Dtos
{
    /// <summary>路由响应。matchMethods 为数组(实体里是 `|` 分隔串)。</summary>
    public sealed class RouteDto
    {
        public long Id { get; set; }
        public string RouteName { get; set; } = "";
        public string ClusterCode { get; set; } = "";
        public string MatchPath { get; set; } = "";
        public string[] MatchMethods { get; set; } = [];
        public string Transforms { get; set; } = "[]";
        public string Remark { get; set; } = "";
        public long CreateDate { get; set; }
        public long ModifyDate { get; set; }
        public string CreatorName { get; set; } = "";
        public string ModifierName { get; set; } = "";
    }

    /// <summary>新增/编辑路由请求。</summary>
    public sealed class RouteUpsertRequest
    {
        public string RouteName { get; set; } = "";
        [Required] public string ClusterCode { get; set; } = "";
        [Required] public string MatchPath { get; set; } = "";
        [Required, MinLength(1)] public string[] MatchMethods { get; set; } = [];
        public string Transforms { get; set; } = "[]";
        public string Remark { get; set; } = "";
    }
}
