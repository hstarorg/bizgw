using System.Text.Json;
using GatewayServer.Data;

namespace GatewayServer.ControlPlane.Config
{
    /// <summary>
    /// 发布前校验:坏配置不下发,避免多实例 reload 集体出问题。
    /// 控制面不依赖 YARP 类型,只做规范化配置的领域校验。
    /// </summary>
    public static class ConfigValidator
    {
        public static List<string> Validate(ConfigSnapshotDoc doc)
        {
            var errors = new List<string>();

            // 集群编码:非空且唯一
            var clusterCodes = new HashSet<string>();
            foreach (var c in doc.Clusters)
            {
                if (string.IsNullOrWhiteSpace(c.ClusterCode))
                {
                    errors.Add($"集群 id={c.Id} 的 cluster_code 为空。");
                    continue;
                }
                if (!clusterCodes.Add(c.ClusterCode))
                {
                    errors.Add($"集群 cluster_code 重复:{c.ClusterCode}。");
                }
            }

            // 路由:必须匹配到已存在的集群、match_path 非空、transforms 可解析
            foreach (var r in doc.Routes)
            {
                if (string.IsNullOrWhiteSpace(r.MatchPath))
                {
                    errors.Add($"路由 id={r.Id} 的 match_path 为空。");
                }
                if (string.IsNullOrWhiteSpace(r.ClusterCode) || !clusterCodes.Contains(r.ClusterCode))
                {
                    errors.Add($"路由 id={r.Id} 指向的集群 cluster_code='{r.ClusterCode}' 不存在。");
                }
                if (!string.IsNullOrWhiteSpace(r.Transforms) && !IsJsonArray(r.Transforms))
                {
                    errors.Add($"路由 id={r.Id} 的 transforms 不是合法的 JSON 数组。");
                }
            }

            return errors;
        }

        private static bool IsJsonArray(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.ValueKind == JsonValueKind.Array;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
