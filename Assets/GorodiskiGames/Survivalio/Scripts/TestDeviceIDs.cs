using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class TestDeviceIDs
    {
        private const string _testDevice = "";

        private static List<string> _list = new()
        { _testDevice };

        public static List<string> List => _list;

        public static bool IsTestDevice()
        {
            var deviceID = SystemInfo.deviceUniqueIdentifier;
            return _list.Contains(deviceID);
        }
    }
}

