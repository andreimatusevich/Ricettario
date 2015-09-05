using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Ricettario;
using ServiceStack.OrmLite;

namespace Ricettario.Controllers
{
    public class Mailer
    {
        public Mailer()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
        }

        public bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // replace with proper validation
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
                return false;
        }

        public void SendEmails(int weekNumber)
        {
            var weekName = weekNumber.ToWeekName();
            var client = new WebClient { Encoding = Encoding.UTF8 };

            var bjs = client.DownloadString(String.Format("http://localhost:56915/PrintShoppingList/{0}/BJs", weekNumber));
            var shoprite = client.DownloadString(String.Format("http://localhost:56915/PrintShoppingList/{0}/ShopRite", weekNumber));
            var menu = client.DownloadString(String.Format("http://localhost:56915/PrintSchedule/{0}", weekNumber));

            Email("BJs - Week of " + weekName, bjs, "andrei.matusevich@gmail.com");
            Email("ShopRite - Week of " + weekName, shoprite, "andrei.matusevich@gmail.com");
            Email("Menu - Week of " + weekName, menu, "andrei.matusevich@gmail.com", "diana.matusevich@gmail.com", "aleklimovich@gmail.com", "antklimovich@gmail.com");//, "tania.a.matusevich@gmail.com");//"aleklimovich@gmail.com");
            //Email("Test Menu - Week of " + week.Name, menu, "andrei.matusevich@gmail.com");
        }

        public void Email(string subject, string body, params string[] emails)
        {
            var fromAddress = new MailAddress("bublegumm@gmail.com", "Weekly Menu Mailer");
            const string fromPassword = "secure1qwedcvxz";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage()
            {
                From = fromAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8
            })
            {
                foreach (var email in emails)
                {
                    message.To.Add(email);
                }

                smtp.Send(message);
            }
        }
    }
}