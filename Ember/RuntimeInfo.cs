
using System.Diagnostics;

namespace Ember;

public static class RuntimeInfo {

    [Conditional("DEBUG")]
    private static void IsDebugCheck(ref bool isDebug) {
        isDebug = true;
    }

    public static bool IsDebug {
        get {
            bool isDebug = false;
            IsDebugCheck(ref isDebug);
            return isDebug;
        }
    }
    
}