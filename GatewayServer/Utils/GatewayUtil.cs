namespace GatewayServer.Utils
{
    public static class GatewayUtil
    {
        /// <summary>
        /// 生成 GUID
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
