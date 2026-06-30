namespace GatewayServer.ConfigProvider
{
    /// <summary>
    /// 本实例身份(单例,启动时定型)。instance_id: env INSTANCE_ID → HOSTNAME → 生成 GUID。
    /// </summary>
    public sealed class InstanceIdentity
    {
        public string InstanceId { get; }
        public string Hostname { get; }
        public long StartedAt { get; }

        public InstanceIdentity()
        {
            var hostnameEnv = Environment.GetEnvironmentVariable("HOSTNAME");
            Hostname = string.IsNullOrWhiteSpace(hostnameEnv) ? Environment.MachineName : hostnameEnv;
            InstanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
                ?? (string.IsNullOrWhiteSpace(hostnameEnv) ? Guid.NewGuid().ToString("N") : hostnameEnv);
            StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
