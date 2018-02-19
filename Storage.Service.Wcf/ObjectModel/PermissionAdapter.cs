using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Service.Wcf
{
    internal class PermissionAdapter
    {
        private bool __init_UserContext;
        private PrincipalContext _UserContext;
        public PrincipalContext UserContext
        {
            get
            {
                if (!__init_UserContext)
                {
                    _UserContext = new PrincipalContext(ContextType.Domain);
                    __init_UserContext = true;
                }
                return _UserContext;
            }
        }

        private bool __init_Users;
        private Dictionary<string, UserPrincipal> _Users;
        private Dictionary<string, UserPrincipal> Users
        {
            get
            {
                if (!__init_Users)
                {
                    _Users = new Dictionary<string, UserPrincipal>();
                    __init_Users = true;
                }
                return _Users;
            }
        }

        private bool __init_Groups;
        private Dictionary<string, GroupPrincipal> _Groups;
        private Dictionary<string, GroupPrincipal> Groups
        {
            get
            {
                if (!__init_Groups)
                {
                    _Groups = new Dictionary<string, GroupPrincipal>();
                    __init_Groups = true;
                }
                return _Groups;
            }
        }

        private UserPrincipal GetPrincipal(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            UserPrincipal principal = null;
            if (this.Users.ContainsKey(userName))
                principal = this.Users[userName];
            else
            {
                principal = UserPrincipal.FindByIdentity(this.UserContext, userName);
                this.Users.Add(userName, principal);
            }

            return principal;
        }

        private GroupPrincipal GetGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentNullException("groupName");

            GroupPrincipal group = null;
            if (this.Groups.ContainsKey(groupName))
                group = this.Groups[groupName];
            else
            {
                group = GroupPrincipal.FindByIdentity(this.UserContext, groupName);
                this.Groups.Add(groupName, group);
            }

            return group;
        }

        public bool IsUserMemberOf(string groupName, string userName)
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentNullException("groupName");

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");

            bool isMember = false;
            UserPrincipal user = this.GetPrincipal(userName);
            if (user == null)
                throw new Exception(string.Format("Не удалось найти пользователя с логином {0}", userName));

            GroupPrincipal group = this.GetGroup(groupName);
            if (group == null)
                throw new Exception(string.Format("Не удалось найти группу пользователей с именем {0}", groupName));

            isMember = user.IsMemberOf(group);

            return isMember;
        }
    }
}