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
    public class DWAccessController : Controller
    {
        public ActionResult Index(string id)
        {
            ViewBag.IsDisabled = "Disabled='Disabled'";
            var dwAccess = new List<BranchDWAccess>();
            ViewBag.ActiveDirectoryGroups = ConfigurationManager.AppSettings["adgroups"];

            if (!string.IsNullOrEmpty(id))
            {
                id = id.Substring(id.LastIndexOf("(") + 2, id.Length - id.LastIndexOf("(") - 4);

                var validateUser = Utilities.FindSingleUser(id);
                if (!validateUser)
                {
                    this.ViewBag.InvaidUser = "user not found in AD";
                    return View(dwAccess);
                }

            

                dwAccess = dbcontext.DefaultSearchBranch(id);

                if(dwAccess.FindAll(w=>w.HasAccess == true).Count > 0)
                {
                    var isUserInGroup = Utilities.CheckUserIsInGroup(id, ConfigurationManager.AppSettings["dwAccessGroupName"]);
                    if (!isUserInGroup)
                    {
                        this.ViewBag.InvaidUser = String.Format("User does not belong to the {0} AD Group. Updating the users permissions will fix this issue", ConfigurationManager.AppSettings["dwAccessGroupName"]);
                    }
                }

                ViewBag.IsDisabled = "";
            }

            return View(dwAccess);
        }

      

        public JsonResult Update(string username, string[] data)
        {
            try
            {
                username = username.Substring(username.LastIndexOf("(") + 2, username.Length - username.LastIndexOf("(") - 4);
                var groupnames = ConfigurationManager.AppSettings["dwAccessGroupName"].Split(',');
                if (data != null && data.Length > 0)
                {
                    foreach(var group in groupnames)
                    Utilities.AddUserToGroup(username, group);
                }
                    
                else
                {
                    foreach (var group in groupnames)
                        Utilities.RemoveUserFromGroup(username, group);
                }
                    

                var rowsAffected = dbcontext.UpdateBranch(username, data);
                return Json(new { success = true, responseText = username + " has been updated" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            

        }
    }
}