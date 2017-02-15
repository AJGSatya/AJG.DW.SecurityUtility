using AJG.DW.SecurityUtility.Web.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using AJG.DW.SecurityUtility.Web.Helpers;
using System;
using System.Configuration;

namespace AJG.DW.SecurityUtility.Web.Controllers
{
    public class PLAccessController : Controller
    {
        public ActionResult Index(string id)
        {
            ViewBag.IsDisabled = "Disabled='Disabled'";

            ViewBag.ActiveDirectoryGroups = ConfigurationManager.AppSettings["adgroups"];
            var dwAccess = new List<BusinessUnitDWAccess>();
            if (!string.IsNullOrEmpty(id))
            {
                id = id.Substring(id.LastIndexOf("(") + 2, id.Length - id.LastIndexOf("(") - 4);
                var validateUser = Utilities.FindSingleUser(id);
                if (!validateUser)
                {
                    this.ViewBag.InvaidUser = "user not found in AD";
                    return View(dwAccess);
                }

              
                dwAccess = dbcontext.DefaultSearchBusinessUnit(id);

                if(dwAccess.FindAll(w=>w.HasAccess== true).Count > 0)
                {
                    var isUserInGroup = Utilities.CheckUserIsInGroup(id, ConfigurationManager.AppSettings["pnLAccessGroupName"]);
                    if (!isUserInGroup)
                    {
                        this.ViewBag.InvaidUser = String.Format("User does not belong to the {0} AD Group. Updating the users permissions will fix this issue", ConfigurationManager.AppSettings["pnLAccessGroupName"]);
                    }

                }

                ViewBag.IsDisabled = "";
            }
           
            return View(dwAccess);
        }
        public ActionResult Contact()
        {
            return View();
        }

   

        public JsonResult Update(string username, string[] data)
        {
            try
            {
                username = username.Substring(username.LastIndexOf("(") + 2, username.Length - username.LastIndexOf("(") - 4);


                var groupnames = ConfigurationManager.AppSettings["pnLAccessGroupName"].Split(',');
                if (data != null && data.Length > 0)
                {
                    foreach (var group in groupnames)
                        Utilities.AddUserToGroup(username, group);
                }

                else
                {
                    foreach (var group in groupnames)
                        Utilities.RemoveUserFromGroup(username, group);
                }


                var rowsAffected = dbcontext.UpdateBusinessUnit(username, data);
                return Json(new { success = true, responseText = username + " has been updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult FindUsers(string search)
        {
            var domain = ConfigurationManager.AppSettings["domain"];

            using (
                var context = new PrincipalContext(ContextType.Domain, domain))
            {
                List<UserPrincipal> searchPrinciples = new List<UserPrincipal>();
                searchPrinciples.Add(new UserPrincipal(context) { SamAccountName = search + "*" });
                searchPrinciples.Add(new UserPrincipal(context) { DisplayName = search + "*" });
                List<Principal> results = new List<Principal>();
                var searcher = new PrincipalSearcher();
                foreach (var item in searchPrinciples)
                {
                    searcher = new PrincipalSearcher(item);
                    results.AddRange(searcher.FindAll());
                }
                var users = new List<AJGUser>();
                foreach (var r in results)
                {
                    var item = r.GetUnderlyingObject() as DirectoryEntry;
                    users.Add(new AJGUser { Id = r.Sid.Value, Name = r.Name, Email = "", SearchDisplayText = r.DisplayName + " - ( " + domain + @"\" + r.SamAccountName + " )" });
                }
                return Json(users, JsonRequestBehavior.AllowGet);
            }
        }
    }
}