using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AJG.DW.SecurityUtility.Web.Helpers
{
    public static class Utilities
    {
        public static string IsActive(this HtmlHelper html,
                                      string control,
                                      string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = control == routeControl &&
                               action == routeAction;

            return returnActive ? "active" : "";
        }

        public static bool FindSingleUser(string id)
        {
            var domain = ConfigurationManager.AppSettings["domain"];
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                List<UserPrincipal> searchPrinciples = new List<UserPrincipal>();
                id = id.Replace(domain + "\\", "");
                searchPrinciples.Add(new UserPrincipal(context) { SamAccountName = id });
                List<Principal> results = new List<Principal>();
                var searcher = new PrincipalSearcher();
                foreach (var item in searchPrinciples)
                {
                    searcher = new PrincipalSearcher(item);
                    results.AddRange(searcher.FindAll());
                }
                return results.Count == 1;
            }
        }
        public static bool CheckUserIsInGroup(string userId, string groupName)
        {
            var domainGroups = ConfigurationManager.AppSettings["domainForGroups"];
            var domainUsers = ConfigurationManager.AppSettings["domain"];

            var adContextUser = ConfigurationManager.AppSettings["adContextUser"];
            var adContextPassword = ConfigurationManager.AppSettings["adContextPassword"];

            userId = userId.Replace(domainUsers + "\\", "");

            using (PrincipalContext pcSMI = new PrincipalContext(ContextType.Domain, domainUsers))
            {
                using (PrincipalContext pcAPAC = new PrincipalContext(ContextType.Domain, domainGroups, adContextUser, adContextPassword))
                {

                    using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(pcSMI, IdentityType.SamAccountName, userId))
                    {
                        if (userPrincipal != null)
                        {
                            using (GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(pcAPAC, groupName))
                            {
                                if (groupPrincipal != null)
                                {
                                    return userPrincipal.IsMemberOf(groupPrincipal);
                                }
                            }
                        }
                    }
                }


                return false;
            }
        }


        public static void AddUserToGroup(string userId, string groupName)
        {
            var domainGroups = ConfigurationManager.AppSettings["domainForGroups"];
            var domainUsers = ConfigurationManager.AppSettings["domain"];

            var adContextUser = ConfigurationManager.AppSettings["adContextUser"];
            var adContextPassword = ConfigurationManager.AppSettings["adContextPassword"];
            userId = userId.Replace(domainUsers + "\\", "");


            using (PrincipalContext pcSMI = new PrincipalContext(ContextType.Domain, domainUsers))
            {
                using (PrincipalContext pcAPAC = new PrincipalContext(ContextType.Domain, domainGroups, adContextUser, adContextPassword))
                {

                    using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(pcSMI, IdentityType.SamAccountName, userId))
                    {
                        if (userPrincipal != null)
                        {
                            using (GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(pcAPAC, groupName))
                            {
                                if (groupPrincipal != null)
                                {
                                    if (!userPrincipal.IsMemberOf(groupPrincipal))
                                    {
                                        string userSid = string.Format("<SID={0}>", userPrincipal.Sid.ToString());
                                        DirectoryEntry groupDirectoryEntry = (DirectoryEntry)groupPrincipal.GetUnderlyingObject();
                                        groupDirectoryEntry.Properties["member"].Add(userSid);
                                        groupDirectoryEntry.CommitChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void RemoveUserFromGroup(string userId, string groupName)
        {
            var domainGroups = ConfigurationManager.AppSettings["domainForGroups"];
            var domainUsers = ConfigurationManager.AppSettings["domain"];

            var adContextUser = ConfigurationManager.AppSettings["adContextUser"];
            var adContextPassword = ConfigurationManager.AppSettings["adContextPassword"];
            userId = userId.Replace(domainUsers + "\\", "");


            using (PrincipalContext pcSMI = new PrincipalContext(ContextType.Domain, domainUsers))
            {
                using (PrincipalContext pcAPAC = new PrincipalContext(ContextType.Domain, domainGroups, adContextUser, adContextPassword))
                {

                    using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(pcSMI, IdentityType.SamAccountName, userId))
                    {
                        if (userPrincipal != null)
                        {
                            using (GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(pcAPAC, groupName))
                            {
                                if (groupPrincipal != null)
                                {
                                    if (userPrincipal.IsMemberOf(groupPrincipal))
                                    {
                                        string userSid = string.Format("<SID={0}>", userPrincipal.Sid.ToString());
                                        DirectoryEntry groupDirectoryEntry = (DirectoryEntry)groupPrincipal.GetUnderlyingObject();
                                        groupDirectoryEntry.Properties["member"].Remove(userPrincipal.DistinguishedName);
                                        groupDirectoryEntry.CommitChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
