using GatewayServer.ControlPlane.DAL;

namespace GatewayServer.ControlPlane.BLL
{
    public class ClusterBll
    {
        private readonly ClusterDal clusterDal;
        public ClusterBll(ClusterDal clusterDal)
        {
            this.clusterDal = clusterDal;
        }

        public Task<bool> DoCreateCluster()
        {
            return Task.FromResult(true);
        } 
    }
}
