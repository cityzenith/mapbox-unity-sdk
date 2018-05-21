using UnityEngine;

namespace Mapbox.Unity.Telemetry
{
	public static class TelemetryFactory
	{
		public static string EventQuery
		{
			get
			{
				if (null == eventQuery)
				{
					if (MapboxProperties.IsUnityEditor || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
						eventQuery = "events=true";
					else
						eventQuery = "events=false";
				}

				return eventQuery;
			}
		}
		private static string eventQuery;

		private static ITelemetryLibrary telemetryInstance;

		public static ITelemetryLibrary GetTelemetryInstance()
		{
			if(null == telemetryInstance)
			{
				if (MapboxProperties.IsUnityEditor)
					telemetryInstance = TelemetryEditor.Instance;
				else if (Application.platform == RuntimePlatform.Android)
					telemetryInstance = TelemetryAndroid.Instance;
				else if (Application.platform == RuntimePlatform.IPhonePlayer)
					telemetryInstance = TelemetryIos.Instance;
				else if (Application.platform == RuntimePlatform.WebGLPlayer)
					telemetryInstance = TelemetryWebgl.Instance;
				else
					telemetryInstance = TelemetryFallback.Instance;
			}

			return telemetryInstance;
		}
	}
}
