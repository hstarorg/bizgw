namespace GatewayServer.ControlPlane.Auth
{
    /// <summary>三类角色。Owner=全权(含用户管理);Editor=配置读写+发布/回滚;Viewer=只读。</summary>
    public static class Roles
    {
        public const string Owner = "Owner";
        public const string Editor = "Editor";
        public const string Viewer = "Viewer";

        /// <summary>可写(增删改 + 发布/回滚)的角色,用于 [Authorize(Roles = Roles.Writers)]。</summary>
        public const string Writers = Owner + "," + Editor;

        public static bool IsValid(string role) => role is Owner or Editor or Viewer;
    }
}
