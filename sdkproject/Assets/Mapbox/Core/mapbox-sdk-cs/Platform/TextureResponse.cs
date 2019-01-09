using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Mapbox.Platform
{
	public class TextureResponse : Response
	{
		public Texture2D Texture { get; private set; }

		public static new TextureResponse FromWebResponse(IAsyncRequest request, UnityWebRequest apiResponse, Exception apiEx)
		{
			TextureResponse textureResponse = FromResponse(Response.FromWebResponse(request, apiResponse, apiEx));

			DownloadHandlerTexture handlerTexture = apiResponse.downloadHandler as DownloadHandlerTexture;

			if (null != handlerTexture)
				textureResponse.Texture = handlerTexture.texture;

			return textureResponse;
		}

		private static TextureResponse FromResponse(Response response)
		{
			TextureResponse textureResponse = new TextureResponse()
			{
				ContentType = response.ContentType,
				Headers = response.Headers,
				Data = response.Data,
				IsUpdate = response.IsUpdate,
				LoadedFromCache = response.LoadedFromCache,
				RequestUrl = response.RequestUrl,
				StatusCode = response.StatusCode,
				Request = response.Request,
				XRateLimitInterval = response.XRateLimitInterval,
				XRateLimitLimit = response.XRateLimitLimit,
				XRateLimitReset = response.XRateLimitReset
			};

			if (null != response.Exceptions)
			{
				foreach (Exception ex in response.Exceptions)
					textureResponse.AddException(ex);
			}

			return textureResponse;
		}
	}
}
