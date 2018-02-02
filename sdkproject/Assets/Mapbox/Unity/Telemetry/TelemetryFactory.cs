namespace Mapbox.Unity.Telemetry
{
	public static class TelemetryFactory
	{
        public static string EventQuery
        {
            get
            {
                return MapboxProperties.HasEvents ? _EventQuery : _NoEventQuery;
            }
        }

        private const string _EventQuery = "events=true";
        private const string _NoEventQuery = "events=false";

		public static ITelemetryLibrary GetTelemetryInstance()
		{
#if UNITY_IOS
			return TelemetryIos.Instance;
#elif UNITY_ANDROID
			return TelemetryAndroid.Instance;
#elif UNITY_WEBGL
			return TelemetryWebgl.Instance;
#else
            if(MapboxProperties.IsUnityEditor)
                return TelemetryEditor.Instance;

            return TelemetryFallback.Instance;
#endif
		}
	}
}