using UnityEngine;
using UnityEngine.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

public static class LogUtils {
	/// <summary>
	/// Log an event for both unity analytics and amplitude.
	/// </summary>
	public static void LogEvent (string eventLine)
	{
//		Amplitude.Instance.logEvent (eventLine);
//
//        Analytics.CustomEvent (eventLine);      
    }

    /// <summary>
    /// Log an event for both unity analytics and amplitude.
    /// </summary>
    public static void LogEvent (string eventLine, string[] eventProperties, bool doesnothing = true)
    {
//      Amplitude.Instance.logEvent (eventLine, eventProperties);
//
//        Analytics.CustomEvent (eventLine, eventProperties);
    }

    /// <summary>
    /// Log an event for both unity analytics and amplitude.
    /// </summary>
    public static void LogEvent (string eventLine, Dictionary<string, string> eventProperties, bool doesnothing = true)
    {
//      Amplitude.Instance.logEvent (eventLine, eventProperties);
//
//        Analytics.CustomEvent (eventLine, eventProperties);
    }

	/// <summary>
	/// Log an event for both unity analytics and amplitude.
	/// </summary>
	public static void LogEvent (string eventLine, Dictionary<string, object> eventProperties, bool doesnothing = true)
	{
//		Amplitude.Instance.logEvent (eventLine, eventProperties);
//
//        Analytics.CustomEvent (eventLine, eventProperties);
	}
}
