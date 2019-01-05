namespace Mapbox.Unity.Telemetry
{
	public static class TelemetryFactory
	{
		public static string EventQuery
		{
			get
			{
				if (string.IsNullOrEmpty(_EventQuery))
					_EventQuery = (MapboxHelper.IsEditor || MapboxHelper.IsIOS || MapboxHelper.IsAndroid) ? "events=true" : "events=false";

				return _EventQuery;
			}
		} 
		private static string _EventQuery;

		private static ITelemetryLibrary telemetryInstance;

		public static ITelemetryLibrary GetTelemetryInstance()
		{
			if (null == telemetryInstance)
			{
				if(MapboxHelper.IsEditor)
					telemetryInstance = TelemetryEditor.Instance;
				else if (MapboxHelper.IsIOS)
					telemetryInstance = TelemetryIos.Instance;
				else if (MapboxHelper.IsAndroid)
					telemetryInstance = TelemetryAndroid.Instance;
				else if (MapboxHelper.IsWebGL)
					telemetryInstance = TelemetryWebgl.Instance;
				else
					telemetryInstance = TelemetryFallback.Instance;
			}

			return telemetryInstance;
		}
	}
}
