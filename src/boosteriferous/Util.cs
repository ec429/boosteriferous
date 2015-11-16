using System;
using System.Collections.Generic;

namespace boosteriferous
{
    public static class Logging
    {
        public static void Message(string message, bool log=true)
        {
            if (log) Log(message, false);
            try
            {
                var msg = new ScreenMessage(message, 4f, ScreenMessageStyle.UPPER_LEFT);
                ScreenMessages.PostScreenMessage(msg);
            }
            catch (Exception)
            {
            }
        }
        public static void Log(string message, bool msg=true)
        {
            UnityEngine.Debug.Log("Bfer: " + (msg ? "" : "Log: ") + message);
            #if DEBUG
			if(msg) Message("Bfer: debug: " + message, false);
            #endif
		}
        public static void Exception(Exception exc)
        {
            Log(exc.Message + '\n' + exc.StackTrace.ToString(), true);
        }

        public static void LogWithUT(string message, bool msg=true)
        {
            string ut = "";
            try
            {
                int t = (int)Planetarium.GetUniversalTime();
                ut = KSPUtil.PrintDate(t, true, true);
            }
            catch(NullReferenceException)
            {
            }
            Log("[" + ut + "] " + message, msg);
        }
    }
}

