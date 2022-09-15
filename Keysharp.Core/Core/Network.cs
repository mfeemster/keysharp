using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Net.Mail;

namespace Keysharp.Core
{
	public static class Network
	{
		/// <summary>
		/// Downloads a resource from the internet.
		/// AHK difference: does not allow specifying flags other than 0.
		/// </summary>
		/// <param name="address">The URI (or URL) of the resource.</param>
		/// <param name="filename">The file path to receive the downloaded data. An existing file will be overwritten.
		/// Leave blank to return the data as a string.
		/// </param>
		/// <returns>The downloaded data if <paramref name="filename"/> is blank, otherwise an empty string.</returns>
		public static string Download(object obj0, object obj1)
		{
			var address = obj0.As();
			var filename = obj1.As();
			var flags = Options.ParseFlags(ref address);

			using (var http = new WebClient())//Obsolete, unsure if Mono supports the new HttpClient though.
			{
				if (flags.Contains("0"))
					http.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

				if (string.IsNullOrEmpty(filename))
					return http.DownloadString(address);

				http.DownloadFile(address, filename);
			}

			return string.Empty;
		}

		/// <summary>
		/// Resolves a host name or IP address.
		/// </summary>
		/// <param name="name">The host name.</param>
		/// <returns>A dictionary with the following key/value pairs:
		/// <list type="bullet">
		/// <item><term>Host</term>: <description>the host name.</description></item>
		/// <item><term>Addresses</term>: <description>the list of IP addresses.</description></item>
		/// </list>
		/// </returns>
		public static Dictionary<string, object> GetHostEntry(string name)
		{
			var entry = Dns.GetHostEntry(name);
			var ips = new string[entry.AddressList.Length];

			for (var i = 0; i < ips.Length; i++)
				ips[i] = entry.AddressList[0].ToString();

			var info = new Dictionary<string, object>();
			info.Add("Host", entry.HostName);
			info.Add("Addresses", ips);
			return info;
		}

		/// <summary>
		/// Sends an email.
		/// </summary>
		/// <param name="recipients">A list of receivers of the message.</param>
		/// <param name="subject">Subject of the message.</param>
		/// <param name="message">Message body.</param>
		/// <param name="options">A dictionary with any the following optional key/value pairs:
		/// <list type="bullet">
		/// <item><term>Attachments</term>: <description>a list of file paths to send as attachments.</description></item>
		/// <item><term>Bcc</term>: <description>a list of blind carbon copy recipients.</description></item>
		/// <item><term>CC</term>: <description>a list of carbon copy recipients.</description></item>
		/// <item><term>From</term>: <description>the from address.</description></item>
		/// <item><term>ReplyTo</term>: <description>the reply address.</description></item>
		/// <item><term>Host</term>: <description>the SMTP client hostname and port.</description></item>
		/// <item><term>(Header)</term>: <description>any additional header and corresponding value.</description></item>
		/// </list>
		/// </param>
		public static void Mail(object recipients, string subject, string message, Map options = null)
		{
			var msg = new MailMessage { Subject = subject, Body = message };
			msg.From = new MailAddress(string.Concat(Environment.UserName, "@", Environment.UserDomainName));

			if (recipients is string S)
				msg.To.Add(new MailAddress(S));
			else if (recipients is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
					if (!string.IsNullOrEmpty(item as string))
						msg.To.Add((string)item);
			}
			else
				return;

			var smtpHost = "localhost";
			int? smtpPort = null;

			if (options == null)
				goto send;

			foreach (var (key, val) in options)
			{
				var item = key as string;

				if (string.IsNullOrEmpty(item))
					continue;

				string[] value;

				if (val is string s)
					value = new[] { s };
				//else if (val is string[] sa)//Probably would never be a raw array of strings.
				//  value = sa;
				else if (val is Array arr)
				{
					value = new string[arr.Count];

					for (var i = 0; i < arr.Count; i++)
						value[i] = arr.array[i].ToString();//Access the underlying ArrayList directly for performance.
				}
				else
					continue;

				switch (item.ToLowerInvariant())
				{
					case Core.Keyword_Attachments:
						foreach (var entry in value)
							if (System.IO.File.Exists(entry))
								msg.Attachments.Add(new Attachment(entry));

						break;

					case Core.Keyword_Bcc:
						foreach (var entry in value)
							msg.Bcc.Add(entry);

						break;

					case Core.Keyword_CC:
						foreach (var entry in value)
							msg.CC.Add(entry);

						break;

					case Core.Keyword_From:
						msg.From = new MailAddress(value[0]);
						break;

					case Core.Keyword_ReplyTo:
						msg.ReplyToList.Add(new MailAddress(value[0]));
						break;

					case Core.Keyword_Host:
					{
						smtpHost = value[0];
						var z = smtpHost.LastIndexOf(Core.Keyword_Port);

						if (z != -1)
						{
							var port = smtpHost.Substring(z + 1);
							smtpHost = smtpHost.Substring(0, z);

							if (int.TryParse(port, out var n))
								smtpPort = n;
						}
					}
					break;

					default:
						msg.Headers.Add(item, value[0]);
						break;
				}
			}

			send:
			var client = smtpPort == null ? new SmtpClient(smtpHost) : new SmtpClient(smtpHost, (int)smtpPort);

			try
			{
				client.Send(msg);
			}
			catch (Exception ex)
			{
				throw new Error(ex.Message);
			}
		}

		public static Array SysGetIPAddresses() => Accessors.A_IPAddress;
	}
}