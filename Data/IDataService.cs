using Renci.SshNet;

namespace coreapi
{
    public interface IDataService
    {
        string AppPath { get; }
        bool Connected { get; }
        string Error { get; set; }
        string LocalAppData { get; }
        SftpClient Sftp { get; set; }
        SshClient Ssh { get; set; }

        bool Connect(string host, int port, string user, string pkey);
        ReturnBox DownloadFile(string src, string dst);
        ReturnBox RunLocal(string cmd);
        ReturnBox RunLocal(string cmd, string args);
        ReturnBox RunRemote(string cmd, int timeout_secs = 3600);
        ReturnBox UploadFile(string src, string dir, string filename);
    }
}