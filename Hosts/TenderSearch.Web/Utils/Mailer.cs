using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TenderSearch.Business.Common.Entities;
using TenderSearch.Contracts.Infrastructure;
using TenderSearch.Data;
using TenderSearch.Data.Security;
using TenderSearch.Web.Configurations;

namespace TenderSearch.Web.Utils
{
    public class Mailer
    {
        public static void SendEmail(string callbackUrl, string username, string address)
        {
            const string lineBreak = "<br>";
            const string qoute = "\"";
            var emailStyleConfig = new EmailStyleConfig();
            var style = emailStyleConfig.Value;

            var messageBody = $"<p style={qoute}{style}{qoute}>" + $"Dear {username},{lineBreak}{lineBreak}" +
                              $"Please reset your password by clicking <a href='{callbackUrl}'>here</a>.{lineBreak}{lineBreak}" +
                              $"Thank you for registering.{lineBreak}{lineBreak}{lineBreak}" +
                              "This is an automated message. Do not reply." + "</p>";
            var message = new MailMessage
            {
                Subject = "TenderSearch Reset Password",
                Body = messageBody
            };


            message.To.Add(new MailAddress(address)); //Todo uncomment after debugging.
            SendEmail(message);
            //Forgot Password Confirmation
        }

        //TODO update urls
        public static string GetUrlLink(Area CurrentStep, string baseAddress, string Id)
        {
            var urlLink = "";
            Area ToStage;
            switch (CurrentStep)
            {

                case Area.Admins:
                    ToStage = Area.Users; //TODO determine the correct value destination
                    urlLink = string.Format("{0}{1}/Txn_Module?ParentId={2}&ReturnToParentListURL={0}{1}/Proposal", baseAddress, ToStage, Id);
                    break;
                case Area.Registration:
                    ToStage = Area.UserManagers;
                    urlLink = string.Format("{0}{1}/AspNetUserRole?ParentId={2}&ReturnToParentListURL={0}{1}/AspNetUser", baseAddress, ToStage, Id);
                    break;
                case Area.Users:
                case Area.UserManagers:
                default:
                    throw new Exception("Method: GetUrlLink. CurrentStep not supported: " + CurrentStep.ToString());

            }

            return urlLink;
        }

        public static void SendEmail(Area FromStage, MailMessage message)
        {

            Area ToStage;
            switch (FromStage)
            {
                case Area.Admins:
                    ToStage = Area.Users;
                    RefillMessageTo(message, ToStage);
                    break;
                case Area.Users:
                    return; //not applicable
                case Area.UserManagers:
                    return; //not applicable
                case Area.Registration:
                    ToStage = Area.UserManagers;
                    RefillMessageTo(message, ToStage);
                    break;
                default:
                    throw new Exception("Parameter is not yet supported: " + FromStage);
            }

            SendEmail(message);
        }

        public static void SendEmail(AspNetUserRole item, string urlLink)
        {
            const string lineBreak = "<br>";
            const string qoute = "\"";
            var message = new MailMessage {Subject = "TenderSearch Permission"};
            var emailStyleConfig = new EmailStyleConfig();
            var style = emailStyleConfig.Value;

            if (string.IsNullOrWhiteSpace(item.OldRole)) //added
            {
                message.Body = $"<p style={qoute}{style}{qoute}>" + $"Dear {item.UserName},{lineBreak}{lineBreak}" +
                               $"The administrator has granted you the role  <b>{item.Role}</b>.{lineBreak}{lineBreak}" +
                               $"Click <a href='{urlLink}'>here</a> to login.{lineBreak}{lineBreak}" +
                               $"Thank you for registering.{lineBreak}{lineBreak}{lineBreak}" +
                               "This is an automated message. Do not reply." + "</p>";
            }
            else //edited
            {
                message.Body = $"<p style={qoute}{style}{qoute}>" + $"Dear {item.UserName},{lineBreak}{lineBreak}" +
                               $"The administrator has changed your role from <b>{item.OldRole}</b> to <b>{item.Role}</b>.{lineBreak}{lineBreak}" +
                               $"Click <a href='{urlLink}'>here</a> to login.{lineBreak}{lineBreak}" +
                               $"If this is not your expected 'role', please contact your TeanderSearch Administrator.{lineBreak}{lineBreak}" +
                               $"Thank you.{lineBreak}{lineBreak}{lineBreak}" +
                               "This is an automated message. Do not reply." + "</p>";
            }
            message.To.Add(new MailAddress(item.Email)); //Todo uncomment after debugging.
            SendEmail(message);
        }

        public static string GetMessageBody(Contract item, string urlLink)
        {
            const string lineBreak = "<br>";
            var emailStyleConfig = new EmailStyleConfig();
            var style = emailStyleConfig.Value;
            return $"<p style='{style}'>" + $"Hi,{lineBreak}{lineBreak}" +
                   $"{item.ContractType} is now ready for processing.{lineBreak}{lineBreak}" +
                   $"Click <a href='{urlLink}'>here</a> to login.{lineBreak}{lineBreak}" +
                   $"Thank you.{lineBreak}{lineBreak}{lineBreak}" + "This is an automated message. Do not reply." + "</p>";
        }

        private static void SendEmail(MailMessage message)
        {
            var smtpFromConfig = new SmtpFromConfig();
            var smtpDisplayNameConfig = new SmtpDisplayNameConfig();
            var smtpHostConfig = new SmtpHostConfig();
            var smtpPortConfig = new SmtpPortConfig();
            var smtpGhostConfig = new SmtpGhostConfig();
            
            var sMTPFrom = smtpFromConfig.Value;
            var sMTPDisplayName = smtpDisplayNameConfig.Value;
            var sMTPHost = smtpHostConfig.Value;
            var sMTPPort = smtpPortConfig.Value;
            var sMTPGhost = smtpGhostConfig.Value;


            if (!string.IsNullOrWhiteSpace(sMTPGhost)) message.Bcc.Add(new MailAddress(sMTPGhost)); //Todo remove email after debugging.


            message.From = new MailAddress(sMTPFrom, sMTPDisplayName, System.Text.Encoding.UTF8);
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                smtp.Port = sMTPPort;
                smtp.Host = sMTPHost;
                //smtp.Send(message);//TODO uncomment when SMSTP server becomes available
            }

        }

        private static void RefillMessageTo(MailMessage message, Area ToStage)
        {
            var emails = GetEmails(ToStage.ToString());

            foreach (var e in emails)
            {
                message.To.Add(new MailAddress(e));
            }
        }
        private static IEnumerable<string> GetEmails(string roleName)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new TenderSearchDb()));
            var role = roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) throw new Exception("Role does not exists: " + roleName);

            var userStore = new UserStore<ApplicationUser>(new TenderSearchDb());
            return userStore.Users
                    .Where(r => r.Roles.Count(x => x.RoleId == role.Id
                                                   && !string.IsNullOrEmpty(r.Email)) > 0)
                    .Select(r => r.Email)
                    .Distinct()
                    .ToList()
                ;
        }
        public static List<AspNetUser> GetUsers()
        {
            var userStore = new UserStore<ApplicationUser>(new TenderSearchDb());

            return userStore.Users
                .OrderBy(r => r.Email)
                .ThenBy(r => r.UserName)
                .Select(r => new AspNetUser { UserName = r.UserName, Email = r.Email })
                .ToList();
        }
    }
}