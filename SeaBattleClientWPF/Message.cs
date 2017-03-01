using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattleClientWPF
{
	public static class Message
	{

		public static void SendMessage(string mailTo, string subject, string message)
		{
			try
			{
				if (string.IsNullOrEmpty(mailTo) || string.IsNullOrEmpty(message)) return;
				MailMessage mail = new MailMessage();
				SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
				mail.From = new MailAddress("email");
				mail.To.Add(mailTo);
				mail.Subject = subject;
				mail.Body = message;
				mail.IsBodyHtml = true;

				smtpServer.Port = 587;
				smtpServer.Credentials = new NetworkCredential("email", "password");
				smtpServer.EnableSsl = true;

				smtpServer.Send(mail);
			}
			catch (Exception err)
			{

			}
		}
	}
}
