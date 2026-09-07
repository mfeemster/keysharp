namespace Keysharp.Builtins
{
	/// <summary>
	/// Public interface for network-related functions.
	/// </summary>
	public partial class Ks
	{
		/// <summary>
		/// Sends an email.
		/// </summary>
		/// <param name="recipients">A list of receivers of the message.</param>
		/// <param name="subject">Subject of the message.</param>
		/// <param name="message">Message body.</param>
		/// <param name="options">A <see cref="Map"/> with any the following optional key/value pairs:<br/>
		/// attachments: A string or <see cref="Array"/> of strings of file paths to send as attachments.<br/>
		/// bcc: A string or <see cref="Array"/> of strings of blind carbon copy recipients.<br/>
		/// cc: A string or <see cref="Array"/> of strings of carbon copy recipients.<br/>
		/// from: A string of comma separated from address.<br/>
		/// replyto: A string of comma separated reply address.<br/>
		/// host: The SMTP client hostname and port string in the form "hostname:port".<br/>
		/// header: A string of additional header information.
		/// </param>
		/// <exception cref="Error">An <see cref="Error"/> exception is thrown if any errors occur.</exception>
		public static object Mail(object recipients, string subject, string message, Map options = null)
		{
			var msg = new MailMessage { Subject = subject, Body = message };
			msg.From = new MailAddress(string.Concat(Environment.UserName, "@", Environment.UserDomainName));

			if (recipients is string s)
			{
				msg.To.Add(new MailAddress(s));
			}
			else if (recipients is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
					if (!string.IsNullOrEmpty(item as string))
						msg.To.Add((string)item);
			}
			else
				return Errors.TypeErrorOccurred(recipients, typeof(IEnumerable));

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

				if (val is string s2)
					value = [s2];
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

				switch (item)
				{
					case var x when x.Equals(Keyword_Attachments, StringComparison.OrdinalIgnoreCase):
						foreach (var entry in value)
							if (File.Exists(entry))
								msg.Attachments.Add(new Attachment(entry));

						break;

					case var x when x.Equals(Keyword_Bcc, StringComparison.OrdinalIgnoreCase):
						foreach (var entry in value)
							msg.Bcc.Add(entry);

						break;

					case var x when x.Equals(Keyword_CC, StringComparison.OrdinalIgnoreCase):
						foreach (var entry in value)
							msg.CC.Add(entry);

						break;

					case var x when x.Equals(Keyword_From, StringComparison.OrdinalIgnoreCase):
						msg.From = new MailAddress(value[0]);
						break;

					case var x when x.Equals(Keyword_ReplyTo, StringComparison.OrdinalIgnoreCase):
						msg.ReplyToList.Add(new MailAddress(value[0]));
						break;

					case var x when x.Equals(Keyword_Host, StringComparison.OrdinalIgnoreCase):
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
				return DefaultObject;
			}
			catch (Exception ex)
			{
				return Errors.ErrorOccurred(ex.Message);
			}
		}

	}

	/// <summary>
	/// Public interface for network-related functions.
	/// </summary>
	public static class Network
	{
		/// <summary>
		/// Downloads a resource from the internet.
		/// AHK difference: does not allow specifying flags other than 0.
		/// </summary>
		/// <param name="url">URL of the file to download, over http, https or ftp.<br/>
		/// For example, "https://someorg.org" might retrieve the welcome page for that organization.<br/>
		/// An ftp URL may carry credentials as "ftp://user:pass@host/path"; without them the login is anonymous.
		/// </param>
		/// <param name="filename">Specify the name of the file to be created locally, which is assumed to be in <see cref="A_WorkingDir"/> if an absolute path isn't specified.<br/>
		/// Any existing file will be overwritten by the new file.<br/>
		/// </param>
		/// <exception cref="OSError">The transfer failed, or the server refused the request.</exception>
		/// <exception cref="ValueError">The URL is not absolute, its scheme is not one of http, https and ftp, or a
		/// cache flag other than <c>*0</c> was given.</exception>
		public static object Download(object url, object filename)
		{
			var address = url.As();
			var file = filename.As();
			var noCache = true;

			if (address.StartsWith('*'))
			{
				var splits = address.Split(SpaceTab);

				if (splits.Length == 2)
				{
					if (splits[0].TrimStart('*').Ai() != 0)
						return Errors.ValueErrorOccurred("Download supports only the *0 cache flag.", splits[0]);

					noCache = false;
					address = splits[1];
				}
			}

			if (!Uri.TryCreate(address, UriKind.Absolute, out var uri))
				return Errors.ValueErrorOccurred($"\"{address}\" is not an absolute URL.");

			// Both paths stream to the file rather than buffering, so a download's size does not become the
			// script's memory, and both wait the way AutoHotkey waits: pumping, so timers and the GUI stay alive.
			if (uri.Scheme == Uri.UriSchemeFtp)
				return Ks.Await(Ks.KeysharpTask.Wrap(FtpToFileAsync(uri, file)));

			if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
				return Ks.Http.DownloadTo(uri, file, noCache);

			return Errors.ValueErrorOccurred($"\"{uri.Scheme}\" is not a supported scheme. Download takes http, https and ftp.");
		}

		/// <summary>How long an FTP transfer may stall before it is abandoned, matching Http's default.</summary>
		private const int FtpTimeoutMs = 30_000;

		/// <summary>
		/// Fetches an ftp URL to a file. A path the server refuses as a plain file is listed instead, which is
		/// what AutoHotkey's WinInet download does for a directory URL.
		/// </summary>
		private static async Task<object> FtpToFileAsync(Uri uri, string path)
		{
			try
			{
				try
				{
					await FtpCopyAsync(uri, WebRequestMethods.Ftp.DownloadFile, path).ConfigureAwait(false);
				}
				catch (WebException ex) when ((ex.Response as FtpWebResponse)?.StatusCode
											  == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					// A listing is small, so it is read whole before the file is touched: an empty one means the
					// path is neither a file nor a directory, and reporting the server's refusal beats leaving a
					// zero-byte file behind and calling it a success.
					var listing = await FtpReadAsync(uri, WebRequestMethods.Ftp.ListDirectoryDetails).ConfigureAwait(false);

					if (listing.Length == 0)
						throw;

					await File.WriteAllBytesAsync(path, listing).ConfigureAwait(false);
				}

				return DefaultObject;
			}
			catch (Exception ex) when (ex is WebException or IOException)
			{
				throw (Exception)new OSError(ex, "Download");
			}
		}

		private static async Task FtpCopyAsync(Uri uri, string method, string path)
		{
			// The response is obtained before the file is opened, so a refused request leaves any existing file alone.
			using var response = await FtpRespondAsync(uri, method).ConfigureAwait(false);
			using var source = response.GetResponseStream();
			using var destination = new FileStream(path, FileMode.Create);
			await source.CopyToAsync(destination).ConfigureAwait(false);
		}

		private static async Task<byte[]> FtpReadAsync(Uri uri, string method)
		{
			using var response = await FtpRespondAsync(uri, method).ConfigureAwait(false);
			using var source = response.GetResponseStream();
			using var buffer = new MemoryStream();
			await source.CopyToAsync(buffer).ConfigureAwait(false);
			return buffer.ToArray();
		}

		/// <summary>
		/// Obtains the response on a pool thread with the synchronous call, which is the only one
		/// <see cref="FtpWebRequest.Timeout"/> applies to: on GetResponseAsync it is documented as having no
		/// effect, and a server that stalls mid-login would hang the transfer for good.
		/// </summary>
		private static Task<WebResponse> FtpRespondAsync(Uri uri, string method)
			=> Task.Run(() => NewFtpRequest(uri, method).GetResponse());

		private static FtpWebRequest NewFtpRequest(Uri uri, string method)
		{
			// FtpWebRequest is the only FTP client in the shared framework. It is obsolete rather than removed, and
			// it is what keeps Download's inherited ftp:// URLs working without taking on a dependency.
#pragma warning disable SYSLIB0014
			var request = (FtpWebRequest)WebRequest.Create(new UriBuilder(uri) { UserName = "", Password = "" }.Uri);
#pragma warning restore SYSLIB0014
			request.Method = method;
			request.UseBinary = true;
			request.KeepAlive = false;
			request.Timeout = FtpTimeoutMs;
			request.ReadWriteTimeout = FtpTimeoutMs;

			if (Ks.Http.UserInfoCredentials(uri) is { } credentials)
				request.Credentials = credentials;

			return request;
		}

		/// <summary>
		/// Returns an <see cref="Array"/> of the system's IPv4 addresses.
		/// </summary>
		/// <returns>An <see cref="Array"/> where each element is an IPv4 address string such as "192.168.0.1".</returns>
		public static Array SysGetIPAddresses()
		{
			var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			var ips = new Array();

			foreach (var address in addresses)
				if (address.AddressFamily == AddressFamily.InterNetwork)
					_ = ips.Push(address.ToString());

			return ips;
		}

		/// <summary>
		/// Internal helper which resolves a host name or IP address.
		/// </summary>
		/// <param name="name">The host name to resolve.</param>
		/// <returns>A <see cref="Dictionary{string, object}"/> with the following key/value pairs:<br/>
		///     Host: The host name.<br/>
		///     Addresses: The list of IP addresses.
		/// </returns>
		internal static Dictionary<string, object> GetHostEntry(string name)
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
	}
}
