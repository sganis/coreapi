#pragma warning disable CS0168
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Renci.SshNet;
using System.Linq;
using Microsoft.Win32;
using System.Reflection;
using System.Net.Sockets;
using Renci.SshNet.Common;
using System.Runtime.Serialization;
using System.Xml;
using System.ServiceProcess;
using Newtonsoft.Json;

namespace coreapi
{

    public class DataService : IDataService
    {
        #region Properties

        public SshClient Ssh { get; set; }
        public SftpClient Sftp { get; set; }
        public string Error { get; set; }
        public bool Connected { get { return Ssh != null && Ssh.IsConnected; } }
        

        private string appPath;
        public string AppPath
        {
            get
            {
                if (appPath == null)
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    appPath = Path.GetDirectoryName(path);
                }
                return appPath;
            }
        }
        private string localAppData;
        public string LocalAppData
        {
            get
            {
                if (localAppData == null)
                    localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\golddrive";
                return localAppData;
            }
        }

        #endregion

        public DataService()
        {

        }



        #region Core Methods

        public bool Connect(string host, int port, string user, string pkey)
        {
            try
            {
                var pk = new PrivateKeyFile(pkey);
                var keyFiles = new[] { pk };
                Ssh = new SshClient(host, port, user, keyFiles);
                Ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                Ssh.Connect();
                Sftp = new SftpClient(host, port, user, keyFiles);
                Sftp.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                Sftp.Connect();

            }
            catch (Renci.SshNet.Common.SshAuthenticationException ex)
            {
                // bad key
                Error = ex.Message;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            return Connected;
        }

        public ReturnBox RunLocal(string cmd)
        {
            // 2 secs slower
            return RunLocal("cmd.exe", "/C " + cmd);
        }

        public ReturnBox RunLocal(string cmd, string args)
        {
            Logger.Log($"Running local command: {cmd} {args}");
            ReturnBox r = new ReturnBox();
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                FileName = cmd,
                Arguments = args
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            r.Output = process.StandardOutput.ReadToEnd();
            r.Error = process.StandardError.ReadToEnd();
            r.ExitCode = process.ExitCode;
            r.Success = r.ExitCode == 0 && String.IsNullOrEmpty(r.Error);
            return r;
        }

        public ReturnBox RunRemote(string cmd, int timeout_secs = 3600)
        {
            ReturnBox r = new ReturnBox();
            if (Connected)
            {
                try
                {
                    SshCommand command = Ssh.CreateCommand(cmd);
                    command.CommandTimeout = TimeSpan.FromSeconds(timeout_secs);
                    r.Output = command.Execute();
                    r.Error = command.Error;
                    r.ExitCode = command.ExitStatus;
                }
                catch (Exception ex)
                {
                    r.Error = ex.Message;
                }
            }
            r.Success = r.ExitCode == 0 && String.IsNullOrEmpty(r.Error);
            return r;
        }

        public ReturnBox DownloadFile(string src, string dst)
        {
            ReturnBox r = new ReturnBox();
            if (Connected)
            {
                try
                {
                    using (Stream fs = File.Create(dst))
                    {
                        Sftp.DownloadFile(src, fs);
                    }
                    r.Success = true;
                }
                catch (Exception ex)
                {
                    r.Error = ex.Message;
                }
            }
            return r;
        }

        public ReturnBox UploadFile(string src, string dir, string filename)
        {
            ReturnBox r = new ReturnBox();
            if (Connected)
            {
                try
                {
                    using (var fs = new FileStream(src, FileMode.Open))
                    {
                        Sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                        Sftp.ChangeDirectory(dir);
                        Sftp.UploadFile(fs, filename, true);
                    }
                    r.Success = true;
                }
                catch (Exception ex)
                {
                    r.Error = ex.Message;
                }
            }
            return r;
        }

        #endregion

       
        //#region SSH Management

        //ReturnBox TestHost(Drive drive)
        //{
        //    ReturnBox r = new ReturnBox();
        //    try
        //    {
        //        using (var client = new TcpClient())
        //        {
        //            var result = client.BeginConnect(drive.Host, drive.Port, null, null);
        //            var success = result.AsyncWaitHandle.WaitOne(5000);
        //            if (!success)
        //            {
        //                r.MountStatus = MountStatus.BAD_HOST;
        //            }
        //            client.EndConnect(result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        r.MountStatus = MountStatus.BAD_HOST;
        //        r.Error = ex.Message;
        //    }
        //    r.MountStatus = MountStatus.OK;
        //    return r;
        //}
        //ReturnBox TestPassword(Drive drive, string password)
        //{
        //    ReturnBox r = new ReturnBox();

        //    if (string.IsNullOrEmpty(password))
        //    {
        //        r.MountStatus = MountStatus.BAD_PASSWORD;
        //        r.Error = "Empty password";
        //        return r;
        //    }
        //    try
        //    {
        //        SshClient client = new SshClient(drive.Host, drive.Port, drive.User, password);
        //        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
        //        client.Connect();
        //        client.Disconnect();
        //        r.MountStatus = MountStatus.OK;
        //    }
        //    catch (Exception ex)
        //    {
        //        r.Error = ex.Message;
        //        if (ex is SshAuthenticationException)
        //        {
        //            r.MountStatus = MountStatus.BAD_PASSWORD;
        //        }
        //        else if (ex is SocketException)
        //        {
        //            r.MountStatus = MountStatus.BAD_HOST;
        //        }
        //        else
        //        {

        //        }

        //    }
        //    return r;
        //}
        //ReturnBox TestSsh(Drive drive)
        //{
        //    ReturnBox r = new ReturnBox();

        //    //r = TestHost(drive);
        //    //if (r.MountStatus == MountStatus.BAD_HOST)
        //    //    return r;

        //    if (!File.Exists(drive.AppKey))
        //    {
        //        r.MountStatus = MountStatus.BAD_PASSWORD;
        //        r.Error = "No ssh key";
        //        return r;
        //    }
        //    try
        //    {
        //        r.MountStatus = MountStatus.UNKNOWN;
        //        var pk = new PrivateKeyFile(drive.AppKey);
        //        var keyFiles = new[] { pk };
        //        SshClient client = new SshClient(drive.Host, drive.Port, drive.User, keyFiles);
        //        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
        //        client.Connect();
        //        client.Disconnect();
        //        r.MountStatus = MountStatus.OK;
        //    }
        //    catch (Exception ex)
        //    {
        //        r.Error = ex.Message;
        //        if (ex is SshAuthenticationException)
        //        {
        //            r.MountStatus = MountStatus.BAD_PASSWORD;
        //        }
        //        else if (ex is SocketException)
        //        {
        //            if (ex.Message.Contains("actively refused it"))
        //                r.MountStatus = MountStatus.BAD_KEY;
        //            else
        //            {
        //                r.MountStatus = MountStatus.BAD_HOST;
        //            }
                    
        //        }
        //        else if (ex is SshConnectionException)
        //        {
        //            r.MountStatus = MountStatus.BAD_HOST;
        //        }
        //        else if (ex is InvalidOperationException)
        //        {
        //            r.MountStatus = MountStatus.BAD_HOST;
        //        }
        //        else
        //        {
        //            if (ex.Message.Contains("milliseconds"))
        //            {
        //                r.Error = "Host does not respond";
        //                r.MountStatus = MountStatus.BAD_HOST;
        //            }
        //            else
        //            {
        //                r.MountStatus = MountStatus.UNKNOWN;

        //            }
        //        }
        //    }
        //    return r;
        //}
        //ReturnBox SetupSsh(Drive drive, string password)
        //{
        //    ReturnBox r = new ReturnBox();
        //    try
        //    {
        //        string pubkey = "";
        //        if (File.Exists(drive.AppKey) && File.Exists(drive.AppPubKey))
        //        {
        //            pubkey = File.ReadAllText(drive.AppPubKey);
        //        }
        //        else
        //        {
        //            pubkey = GenerateKeys(drive);
        //        }
        //        SshClient client = new SshClient(drive.Host, drive.Port, drive.User, password);
        //        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
        //        client.Connect();
        //        string cmd = "";
        //        //bool linux = false;
        //        //if(linux)
        //            cmd = $"exec sh -c \"cd; umask 077; mkdir -p .ssh; echo '{pubkey}' >> .ssh/authorized_keys\"";
        //        //else
        //        //    cmd = $"mkdir %USERPROFILE%\\.ssh 2>NUL || echo {pubkey.Trim()} >> %USERPROFILE%\\.ssh\\authorized_keys";
        //        SshCommand command = client.CreateCommand(cmd);
        //        command.CommandTimeout = TimeSpan.FromSeconds(5);
        //        r.Output = command.Execute();
        //        r.Error = command.Error;
        //        r.ExitCode = command.ExitStatus;
        //    }
        //    catch (Exception ex)
        //    {
        //        r.Error = ex.Message;
        //        return r;
        //    }

        //    r = TestSsh(drive);
        //    if (r.MountStatus != MountStatus.OK)
        //        return r;

        //    return r;
        //}
        //string GenerateKeys(Drive drive)
        //{
        //    string pubkey = "";
        //    try
        //    {
        //        string dotssh = $@"{drive.UserProfile}\.ssh";
        //        if (!Directory.Exists(dotssh))
        //            Directory.CreateDirectory(dotssh);
        //        ReturnBox r = RunLocal($@"""{AppPath}\ssh-keygen.exe""", $@"-m PEM -t rsa -N """" -f ""{drive.AppKey}""");
        //        if (File.Exists(drive.AppPubKey))
        //            pubkey = File.ReadAllText(drive.AppPubKey).Trim();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log("Error generating keys: " + ex.Message);
        //    }
        //    return pubkey;

        //}

        //#endregion

     

    }
}

