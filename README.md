# DynamicFinder
A DLL sideloading utility.

DynamicFinder will search every running process for DLL sideloading opportunities.

Dynamic Finder will create proxy DLLs for any potencial targets.

```
Usage: DynamicFinder [User|Group]
PS C:\DynamicFinder> dotnet run Administrators
[*] Running...
[*] There are 96 directories from running applications we can write to.
[+] There are 1 DLLs we can use for C:\Windows\system32\wbem\unsecapp.exe
        - wbemcomn.dll
[+] There are 2 DLLs we can use for C:\Windows\system32\wbem\wmiprvse.exe
        - NCObjAPI.DLL
        - wbemcomn.dll
        
PS C:\DynamicFinder> dotnet run Users
[*] Running...
[*] There are 2 directories from running applications we can write to.
[+] There are 5 DLLs we can use for C:\Program Files (x86)\Steam\steam.exe
        - COMCTL32.dll
        - VERSION.dll
        - CRYPT32.dll
        - WSOCK32.dll
        - bcrypt.dll
[+] There are 3 DLLs we can use for C:\Program Files (x86)\Steam\bin\cef\cef.win7x64\steamwebhelper.exe
        - WINMM.dll
        - dbghelp.dll
        - bcrypt.dll
```

```
[*] Created proxy for UIAutomationCore.c
```

```cpp
// UIAutomationCore.c
#include <Windows.h>


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#pragma comment(linker,"/export:DllGetActivationFactory=C:\\Windows\\System32\\UIAutomationCore.DLL.DllGetActivationFactory,@1")
#pragma comment(linker,"/export:DllCanUnloadNow=C:\\Windows\\System32\\UIAutomationCore.DLL.DllCanUnloadNow,@2")
#pragma comment(linker,"/export:DllGetClassObject=C:\\Windows\\System32\\UIAutomationCore.DLL.DllGetClassObject,@3")
// ...
```


This is a rewrite of a tool I'd previously written.
