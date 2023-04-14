# DynamicFinder
A DLL sideloading utility.

DynamicFinder will search every running process for DLL sideloading opportunities.

```
Usage: DynamicFinder [User|Administrators|System]
PS C:\GitHub\DynamicFinder\DynamicFinder> dotnet run Administrators
[*] Running...
[*] There are 96 directories from running applications we can write to.
[+] There are 1 DLLs we can use for C:\Windows\system32\wbem\unsecapp.exe
        - wbemcomn.dll
[+] There are 2 DLLs we can use for C:\Windows\system32\wbem\wmiprvse.exe
        - NCObjAPI.DLL
        - wbemcomn.dll
```

This is a rewrite of a tool I'd previously written.
