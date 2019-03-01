using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

 using System.IO;
 using System.Text;
  

namespace OpenPTrack
{
	public class MyLogHandler : ILogHandler
	{
		public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
		{
			UnityEngine.Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			UnityEngine.Debug.unityLogger.LogException(exception, context);
		}
	}

    /// <summary>
    /// A Logger for OpenPTrack Unity Projects, adds an "OpenPTrack" tag and the current method name to the message
    /// </summary>
    /// <author>
    /// Carlo Rizzardo
    /// </author>
    public class OptLogger
	{
		private static UnityEngine.Logger myLogger = new UnityEngine.Logger(new MyLogHandler());
		private static string APP_ID = "OpenPTrack";
		
		private static string getCallerName()
		{
			var st = new StackTrace();
			var sf = st.GetFrame(2);
			return sf.GetMethod().DeclaringType.FullName+"."+sf.GetMethod().Name;
		}
		public static void info(string msg)
		{
			//Console.WriteLine("I: "+APP_ID + ": " + getCallerName()+": "+msg);
			myLogger.Log(APP_ID, getCallerName()+": "+msg);
		}
		public static void warn(string msg)
		{
			//Console.WriteLine("W: "+APP_ID + ": " + getCallerName()+": "+msg);
			myLogger.LogWarning(APP_ID, getCallerName() + ": " + msg);
		}
		public static void error(string msg)
		{
			//Console.WriteLine("E: "+APP_ID + ": " + getCallerName()+": "+msg);
			myLogger.LogError(APP_ID, getCallerName() + ": " + msg);
		}
		public static void exception(Exception exception)
		{
			myLogger.LogException(exception);
		}

		public static void init()
		{
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			UnitySystemConsoleRedirector.Redirect();
		}
	}



  
 /// <summary>
 /// Redirects writes to System.Console to Unity3D's Debug.Log.
 /// </summary>
 /// <author>
 /// Jackson Dunstan, http://jacksondunstan.com/articles/2986
 /// </author>
 public static class UnitySystemConsoleRedirector
 {
     private class UnityTextWriter : TextWriter
     {
         private StringBuilder buffer = new StringBuilder();
  
         public override void Flush()
         {
             OptLogger.info(buffer.ToString());
             buffer.Length = 0;
         }
  
         public override void Write(string value)
         {
             buffer.Append(value);
             if (value != null)
             {
                 var len = value.Length;
                 if (len > 0)
                 {
                     var lastChar = value [len - 1];
                     if (lastChar == '\n')
                     {
                         Flush();
                     }
                 }
             }
         }
  
         public override void Write(char value)
         {
             buffer.Append(value);
             if (value == '\n')
             {
                 Flush();
             }
         }
  
         public override void Write(char[] value, int index, int count)
         {
             Write(new string (value, index, count));
         }
  
         public override Encoding Encoding
         {
             get { return Encoding.Default; }
         }
     }
  
     public static void Redirect()
     {
         Console.SetOut(new UnityTextWriter());
     }
 }
}