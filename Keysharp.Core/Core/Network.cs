using static Keysharp.Scripting.Keywords;

namespace Keysharp.Core
{
	public static class Network
	{
		/// <summary>
		/// Downloads a resource from the internet.
		/// AHK difference: does not allow specifying flags other than 0.
		/// </summary>
		/// <param name="address">The URI (or URL) of the resource.</param>
		/// <param name="filename">The file path to receive the downloaded data. An existing file will be overwritten.</param>
		public static void Download(object obj0, object obj1)
		{
			var address = obj0.As();
			var filename = obj1.As();
			var flags = -1;

			if (address.StartsWith('*'))
			{
				var splits = address.Split(SpaceTab);

				if (splits.Length == 2)
				{
					flags = splits[0].TrimStart('*').Ai();
					address = splits[1];
				}
			}

			var t = Task.Run(async () =>//We explicitly do NOT use Task.Factory.StartNew() here, because it does not understand async delegates.
			{
				try
				{
					using (var client = new HttpClient())
					{
						if (flags != 0)
						{
							client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
							{
								NoCache = true
							};
						}

						var uri = new Uri(address);

						using (var response = await client.GetStreamAsync(address))
						{
							using (var fs = new FileStream(filename, FileMode.Create))
							{
								await response.CopyToAsync(fs);
								return true;
							}
						}
					}
				}
				catch (Exception)
				{
					throw;//Do not pass ex because it will reset the stack information.
				}
			});
			t.Wait();
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

			var info = new Dictionary<string, object>
			{
				{ "Host", entry.HostName },
				{ "Addresses", ips }
			};
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
					value = [s];
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
					case Keyword_Attachments:
						foreach (var entry in value)
							if (File.Exists(entry))
								msg.Attachments.Add(new Attachment(entry));

						break;

					case Keyword_Bcc:
						foreach (var entry in value)
							msg.Bcc.Add(entry);

						break;

					case Keyword_CC:
						foreach (var entry in value)
							msg.CC.Add(entry);

						break;

					case Keyword_From:
						msg.From = new MailAddress(value[0]);
						break;

					case Keyword_ReplyTo:
						msg.ReplyToList.Add(new MailAddress(value[0]));
						break;

					case Keyword_Host:
					{
						smtpHost = value[0];
						var z = smtpHost.LastIndexOf(Keyword_Port);

						if (z != -1)
						{
							var port = smtpHost.AsSpan(z + 1);
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