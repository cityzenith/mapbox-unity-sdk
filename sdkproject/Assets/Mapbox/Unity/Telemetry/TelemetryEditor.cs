﻿namespace Mapbox.Unity.Telemetry
{
	using System.Collections.Generic;
	using System.Collections;
	using Mapbox.Json;
	using System;
	using Mapbox.Unity.Utilities;
	using UnityEngine;
	using System.Text;
    using System.Reflection;

    public class TelemetryEditor : ITelemetryLibrary
	{
		string _url;

		static ITelemetryLibrary _instance;
		public static ITelemetryLibrary Instance
		{
			get
			{
                if (null == _instance)
                    _instance = new TelemetryEditor();

                return _instance;
			}
		}

        private static string applicationIdentifier;
        private static string bundleVersion;
        private static string bundleCode;

        private TelemetryEditor()
        {
            string assemblyPath = Assembly.GetAssembly(typeof(Vector3)).Location; // We get the location of the UnityEngine Assembly
            assemblyPath = assemblyPath.Replace("UnityEngine", "UnityEditor"); //UnityEditor is in the same path

            //We load the editor assembly and get the method we need 
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            Type type = assembly.GetType("UnityEditor.PlayerSettings");

            applicationIdentifier = type.GetProperty("applicationIdentifier").GetValue(null, null).ToString();
            bundleVersion = type.GetProperty("bundleVersion").GetValue(null, null).ToString();
            bundleCode = "0";

#if UNITY_IOS
            type = assembly.GetType("UnityEditor.PlayerSettings.iOS");
            bundleCode = type.GetProperty("buildNumber").GetValue(null, null).ToString();
#elif UNITY_ANDROID
            type = assembly.GetType("UnityEditor.PlayerSettings.Android");
            bundleCode = type.GetProperty("bundleVersionCode").GetValue(null, null).ToString();
#endif
        }

        public void Initialize(string accessToken)
		{
			_url = string.Format("{0}events/v2?access_token={1}", Mapbox.Utils.Constants.EventsAPI, accessToken);
		}

		public void SendTurnstile()
		{
			// This is only needed for maps at design-time.
			//Runnable.EnableRunnableInEditor();

			var ticks = DateTime.Now.Ticks;
			if (ShouldPostTurnstile(ticks))
			{
				Runnable.Run(PostWWW(_url, GetPostBody()));
				PlayerPrefs.SetString(Constants.Path.TELEMETRY_TURNSTILE_LAST_TICKS_EDITOR_KEY, ticks.ToString());
			}
		}

		string GetPostBody()
		{
			List<Dictionary<string, object>> eventList = new List<Dictionary<string, object>>();
			Dictionary<string, object> jsonDict = new Dictionary<string, object>();

			long unixTimestamp = (long)Mapbox.Utils.UnixTimestampUtils.To(DateTime.UtcNow);

			jsonDict.Add("event", "appUserTurnstile");
			jsonDict.Add("created", unixTimestamp);
			jsonDict.Add("userId", SystemInfo.deviceUniqueIdentifier);
			jsonDict.Add("enabled.telemetry", false);
			eventList.Add(jsonDict);

			var jsonString = JsonConvert.SerializeObject(eventList);
			return jsonString;
		}

		bool ShouldPostTurnstile(long ticks)
		{
			var date = new DateTime(ticks);
			var longAgo = DateTime.Now.AddDays(-100).Ticks.ToString();
			var lastDateString = PlayerPrefs.GetString(Constants.Path.TELEMETRY_TURNSTILE_LAST_TICKS_EDITOR_KEY, longAgo);
			long lastTicks = 0;
			long.TryParse(lastDateString, out lastTicks);
			var lastDate = new DateTime(lastTicks);
			var timeSpan = date - lastDate;
			return timeSpan.Days >= 1;
		}

		IEnumerator PostWWW(string url, string bodyJsonString)
		{
			byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
			var headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");
			headers.Add("user-agent", GetUserAgent());

			var www = new WWW(url, bodyRaw, headers);
			yield return www;
		}

		static string GetUserAgent()
		{
            string userAgent = string.Format("{0}/{1}/{2} MapboxEventsUnityEditor/{3}",
                                          applicationIdentifier,
                                          bundleVersion,
                                          bundleCode,
                                          Constants.SDK_VERSION
                                         );

            /*var userAgent = string.Format("{0}/{1}/{2} MapboxEventsUnityEditor/{3}",
										  PlayerSettings.applicationIdentifier,
										  PlayerSettings.bundleVersion,
#if UNITY_IOS
										  PlayerSettings.iOS.buildNumber,
#elif UNITY_ANDROID
										  PlayerSettings.Android.bundleVersionCode,
#else
										  "0",
#endif
										  Constants.SDK_VERSION
										 );
                                         */
            return userAgent;
		}

		public void SetLocationCollectionState(bool enable)
		{
			// Empty.
		}
    }
}