using coreapi.Models;
using Renci.SshNet;

namespace coreapi.Data
{
    public interface IDataService
    {
        string AppPath { get; }
        string Error { get; set; }
        SshClient Connect(string host, int port, string user, string password, string pkey);
        ReturnBox RunRemote(SshClient ssh, string cmd, int timeout_secs = 3600);
        ReturnBox SubscribeLinux(UserModel user);
        ReturnBox UnsubscribeLinux(UserModel user);
        UserModel GetUser(string username);
    }
}