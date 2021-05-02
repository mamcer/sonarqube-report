using System;
using System.Net;
using System.Net.Mail;

namespace SonarQube.Application
{
    public class EmailService
    {
        private string _host;
        private int _port;
        private string _userName;
        private string _password;
        private bool _enableSsl;
        private bool _useDefaultCredentials;        

        public EmailService(string host, int port, string userName, string password, bool enableSsl, bool useDefaultCredentials)
        {
            _host = host;
            _port = port;
            _userName = userName;
            _password = password;
            _enableSsl = enableSsl;
            _useDefaultCredentials = useDefaultCredentials;
        }

        public void SendEmail(string to, string projectKey, string body)
        {
            var subject = $"[SonarQube] {projectKey} - {DateTime.Today.ToString("MMMM dd, yyyy", new System.Globalization.CultureInfo("en-US"))}";

            using (SmtpClient smtp = new SmtpClient(_host, _port))
            {
                smtp.EnableSsl = _enableSsl;
                smtp.UseDefaultCredentials = _useDefaultCredentials;
                smtp.Credentials = new NetworkCredential(_userName, _password);
                var mailMessage = new MailMessage(_userName, to, subject, body)
                {
                    IsBodyHtml = true
                };

                smtp.Send(mailMessage);
            }
        }
    }
}