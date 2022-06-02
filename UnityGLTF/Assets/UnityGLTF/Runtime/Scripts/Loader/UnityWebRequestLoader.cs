﻿using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityGLTF.Loader
{
	public class UnityWebRequestLoader : IDataLoader
	{
		private string dir;
		private NetworkCredential credentials;

		public UnityWebRequestLoader(string dir)
		{
			this.dir = dir;
		}

		public UnityWebRequestLoader(string dir, NetworkCredential newCredentials)
		{
			this.dir = dir;
			this.credentials = newCredentials;
		}

		public async Task<Stream> LoadStreamAsync(string relativeFilePath)
		{
			var path = Path.Combine(dir, relativeFilePath).Replace("\\","/");
			if (File.Exists(path))
				path = "file://" + Path.GetFullPath(path);
			var request = UnityWebRequest.Get(path);
			if (credentials != null)
			{
				string auth = Authenticate(credentials.UserName, credentials.Password);
				request.SetRequestHeader("AUTHORIZATION", auth);
			}
			// request.downloadHandler = new DownloadStreamHandler(new byte[1024 * 1024]);
			var asyncOperation = request.SendWebRequest();

			while (!asyncOperation.isDone) {
				await Task.Yield();
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Error when loading {relativeFilePath} ({path}): {request.error}");
				return null;
			}

			var results = request.downloadHandler.data;
			var stream = new MemoryStream(results, 0, results.Length, false, true);
			return stream;
		}

		private string Authenticate(string username, string password)
		{
			string auth = username + ":" + password;
			auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
			auth = "Basic " + auth;
			return auth;
		}

		// TODO: figure out how to do this correctly in a streaming fashion.
		// private class DownloadStreamHandler : DownloadHandlerScript
		// {
		// 	private ulong expectedTotalBytes = 0;
		// 	private Stream stream;
		// 	public Stream GetStream()
		// 	{
		// 		return stream;
		// 	}
		//
		// 	public DownloadStreamHandler(byte[] buffer) : base(buffer)
		// 	{
		// 		stream = new MemoryStream(buffer, true);
		// 	}
		//
		// 	protected override bool ReceiveData(byte[] data, int dataLength)
		// 	{
		// 		if(data == null || data.Length < 1)
		// 		{
		// 			Debug.Log("LoggingDownloadHandler :: ReceiveData - received a null/empty buffer");
		// 			return false;
		// 		}
		//
		// 		stream.Write(data, 0, dataLength);
		// 		return true;
		// 	}
		//
		// 	protected override byte[] GetData()
		// 	{
		// 		throw new System.NotSupportedException("This is a stream, can't get all bytes");
		// 	}
		//
		// 	protected override void CompleteContent()
		// 	{
		// 		Debug.Log("LoggingDownloadHandler :: CompleteContent - DOWNLOAD COMPLETE!");
		// 	}
		//
		// 	protected override void ReceiveContentLengthHeader(ulong contentLength)
		// 	{
		// 		Debug.Log(string.Format("LoggingDownloadHandler :: ReceiveContentLength - length {0}", contentLength));
		// 		expectedTotalBytes = contentLength;
		// 		stream.SetLength((long) contentLength);
		// 	}
		// }
	}
}
