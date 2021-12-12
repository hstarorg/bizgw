using SqlSugar;

namespace GatewayServer.ConfigrationAPI.DAL
{
    public class DataAccessBase
    {
        protected readonly SqlSugarScope db;
        public DataAccessBase()
        {
            db = new SqlSugarScope(new ConnectionConfig()
            {
                // server=192.168.31.250;port=3306;uid=root;pwd=localDev;database=gatewaydb
                ConnectionString = Environment.GetEnvironmentVariable("ConnectionString"), // 连接符字串
                DbType = DbType.MySql,//数据库类型
                IsAutoCloseConnection = true //不设成true要手动close
            });
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql);//输出sql,查看执行sql
            };
        }
    }
}
