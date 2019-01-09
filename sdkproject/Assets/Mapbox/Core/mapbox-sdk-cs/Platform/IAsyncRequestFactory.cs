//-----------------------------------------------------------------------
// <copyright file="IAsyncRequest.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Platform {

	using Mapbox.Map;
    using Mapbox.Unity.Utilities;
    using System;
	using UnityEngine;
	using UnityEngine.Networking;

	/// <summary> A handle to an asynchronous request. </summary>
	public static class IAsyncRequestFactory {

		public static IAsyncRequest CreateRequest(
			string url
			, Action<Response> callback
			, int timeout
			, HttpRequestType requestType= HttpRequestType.Get
		) {
			return new Mapbox.Unity.Utilities.HTTPRequest(url, callback, timeout, requestType);
		}

		public static IAsyncRequest CreateUnityRequest(
			string url
			, Action<Response> callback
			, int timeout
			, HttpRequestType requestType = HttpRequestType.Get
			, DownloadHandler downloadHandler = null
		)
		{
			return new Mapbox.Unity.Utilities.HTTPRequest(url, callback, timeout, requestType, downloadHandler);
		}
	}
}
