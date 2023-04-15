# DynamicFinder
A DLL sideloading utility.

DynamicFinder will search every running process for DLL sideloading opportunities.

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

This is a rewrite of a tool I'd previously written.
