namespace GatewayServer.ConfigrationAPI.DAL
{
    public class ClusterDal : DataAccessBase
    {
        public IList<string> QueryClustersAsync()
        {
            return new string[] { "" }.ToList();
        }
    }
}
