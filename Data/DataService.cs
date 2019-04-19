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
using coreapi.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using coreapi.Utilities;

namespace coreapi.Data
{

    public class DataService : IDataService
    {
        #region Properties

        public string Error { get; set; }
        public string LinuxHost { get; set; }
        
        public IConfiguration Configuration { get; set; }

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

        Dictionary<string, UserModel> Users { get; set; }
        IDataProtector _protector;

        #endregion

        public DataService()
        {
            // fixme: get this from persistent storage and protected 
            Users = new Dictionary<string, UserModel>();
            var provider = DataProtectionProvider.Create("corepai");
            _protector = provider.CreateProtector("encryption");

        }



        #region Core Methods

        public SshClient Connect(string host, int port, string user, string password, string pkey)
        {
            SshClient ssh = null;
            try
            {
                if(password != null)
                {
                    ssh = new SshClient(host, port, user, password);
                    ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                }
                else
                {
                    var pk = new PrivateKeyFile(pkey);
                    var keyFiles = new[] { pk };
                    ssh = new SshClient(host, port, user, keyFiles);
                    ssh.ConnectionInfo.Timeout = TimeSpan.FromSeconds(5);
                }
                ssh.Connect();
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
            return ssh;
        }

        public ReturnBox RunRemote(SshClient ssh, string cmd, int timeout_secs = 3600)
        {
            ReturnBox r = new ReturnBox();
            if (ssh.IsConnected)
            {
                try
                {
                    SshCommand command = ssh.CreateCommand(cmd);
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


        #endregion

        public UserModel GetUser(string username)
        {
            if (Users.ContainsKey(username))
            {
                return Users[username];
            }
            return new UserModel { Username = username };
        }

        public ReturnBox SubscribeLinux(UserModel user)
        {
            UserModel u = GetUser(user.Username);
            if (u.IsSubscribedLinux)
                return new ReturnBox { Error = "user already subscribed" };
            var ssh = Connect(LinuxHost, 22, Util.GetLogin(user.Username), user.LinuxPassword, "");
            if (ssh != null && ssh.IsConnected)
            {
                user.Ssh = ssh;
                user.IsSubscribedLinux = true;
                user.LinuxPassword = _protector.Protect(user.LinuxPassword);
                string p = _protector.Unprotect(user.LinuxPassword);
                Users[user.Username] = user;
                return new ReturnBox { Success = true } ;
            }
            return new ReturnBox { Error = Error }; ;
        }
        public ReturnBox UnsubscribeLinux(UserModel user)
        {
            UserModel u = GetUser(user.Username);
            u.IsSubscribedLinux = false;
            u.LinuxPassword = null;
            u.Ssh.Disconnect();
            u.Ssh.Dispose();
            return new ReturnBox { Success = true }; 
        }




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

