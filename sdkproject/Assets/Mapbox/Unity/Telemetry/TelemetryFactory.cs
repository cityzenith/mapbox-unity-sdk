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
					if (MapboxProperties.IsEditor || MapboxProperties.IsAndroid || MapboxProperties.IsIOS)
						eventQuery = "events=true";
					else
						eventQuery = "events=false";
				}

				return eventQuery;
			}
		}
		public static string eventQuery;

		public static ITelemetryLibrary GetTelemetryInstance()
		{
			if(MapboxProperties.IsEditor)
				return TelemetryEditor.Instance;
			else if(MapboxProperties.IsIOS)
				return TelemetryIos.Instance;
			else if(MapboxProperties.IsAndroid)
				return TelemetryAndroid.Instance;
			else if(MapboxProperties.IsWebGl)
				return TelemetryWebgl.Instance;
			else
				return TelemetryFallback.Instance;
		}
	}
}
