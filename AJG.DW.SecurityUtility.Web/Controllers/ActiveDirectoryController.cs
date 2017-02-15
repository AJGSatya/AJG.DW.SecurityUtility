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
    public class ActiveDirectoryController : Controller
    {
     

        public ActionResult Index(string id)
        {
            ViewBag.IsDisabled = "Disabled='Disabled'";
            var groups = new List<ActiveDirectoryGroupItem>();
            if (!string.IsNullOrEmpty(id))
            {
                id = id.Substring(id.LastIndexOf("(") + 2, id.Length - id.LastIndexOf("(") - 4);

                var validateUser = Utilities.FindSingleUser(id);
                if (!validateUser)
                {
                    this.ViewBag.InvaidUser = "user not found in AD";
                    return View();
                }

                var checkGroups = ConfigurationManager.AppSettings["adgroups"];

                foreach (var groupName in checkGroups.Split(','))
                {
                   var inGroup = Utilities.CheckUserIsInGroup(id, groupName);
                    groups.Add(new ActiveDirectoryGroupItem
                    {
                        Name = groupName,
                        Value = inGroup
                    });
                }


                ViewBag.ActiveDirectoryGroups = groups;

                ViewBag.IsDisabled = "";
            }

            return View();
        }

      

        public JsonResult Update(string username, List<ActiveDirectoryGroupItem> adgroups)
        {
            try
            {
                username = username.Substring(username.LastIndexOf("(") + 2, username.Length - username.LastIndexOf("(") - 4);

                foreach (var group in adgroups)
                {
                    if (group.Value == true)
                        Utilities.AddUserToGroup(username, group.Name);
                    else
                        Utilities.RemoveUserFromGroup(username, group.Name);
                }

                return Json(new { success = true, responseText = username + " has been updated" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, responseText = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            

        }
    }
}