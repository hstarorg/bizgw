namespace GatewayServer.ControlPlane.DAL
{
    public class ClusterDal
    {
        public IList<string> QueryClustersAsync()
        {
            return new string[] { "" }.ToList();
        }
    }
}
