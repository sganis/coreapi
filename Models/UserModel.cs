using Microsoft.AspNetCore.Mvc.ModelBinding;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coreapi.Models
{
    public class UserModel
    {
        [BindNever]
        public SshClient Ssh { get; set; }

        public string Username { get; set; }

        public bool IsSubscribedLinux { get; set; }

        public bool IsConnectedToLinux { get { return Ssh != null && Ssh.IsConnected;  } }

        [Required(ErrorMessage = "Linux password is required")]
        [StringLength(8, ErrorMessage = "Minimun length is 8")]
        [Display(Prompt = "Linux password")]
        [DataType(DataType.Password)]
        public string LinuxPassword { get; set; }

        public UserModel()
        {

        }

    }
}
