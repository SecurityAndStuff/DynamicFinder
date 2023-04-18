module Dll

open System
open System.IO
open Microsoft.Win32
open System.Security.AccessControl
open System.DirectoryServices.AccountManagement

let knownDlls =
    let keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs"
    let key = (Registry.LocalMachine.OpenSubKey keyName)

    key.GetValueNames()
    |> Seq.map key.GetValue
    |> Seq.map (fun dll -> (string dll).ToLower())


let getDlls (path: string) =
    let dllFilters = [ "ntdll.dll" ]

    try
        let pe = PeNet.PeFile(path)
        let isApiSet (name: string) = name.ToLower().StartsWith "api-ms"

        let isKnownDll (name: string) =
            knownDlls |> Seq.contains (name.ToLower())

        let dlls =
            match pe.ImportedFunctions with
            | null -> Seq.empty
            | functions -> functions
            |> Seq.filter (fun func -> not (isNull func))
            |> Seq.map (fun f -> f.DLL)
            |> Seq.filter (fun dll -> not (isNull dll))
            |> Seq.distinct
            |> Seq.filter (fun dll -> not (isApiSet dll || isKnownDll dll || Seq.contains dll dllFilters))
            |> Seq.filter (fun dll -> not (File.Exists($@"{Path.GetDirectoryName(path)}\{dll}")))

        dlls
    with _ ->
        []

let canWriteToDirectory (directory, user) =
    try
        let directorySecurity = DirectorySecurity(directory, AccessControlSections.Access)

        let accessRules =
            directorySecurity.GetAccessRules(true, true, typeof<System.Security.Principal.SecurityIdentifier>)

        let context = new PrincipalContext(ContextType.Machine)

        [ for rule in accessRules do
              let rule = rule :?> FileSystemAccessRule
              let sid = rule.IdentityReference.Value
              let principal = Principal.FindByIdentity(context, IdentityType.Sid, sid)

              if
                  rule.FileSystemRights.HasFlag(FileSystemRights.Write)
                  && principal.Name.ToLower() = user
              then
                  rule ]
            .Length
        <> 0
    with
    | :? UnauthorizedAccessException
    | :? InvalidCastException
    | :? ArgumentException as ex -> false

let proxies: string[] = [||]

let proxyReplace (exports: seq<PeNet.Header.Pe.ExportFunction>, path: string) =
    Seq.map
        (fun (export: PeNet.Header.Pe.ExportFunction) ->
            let formatted =
                $@"#pragma comment(linker,""/export:%s{export.Name}=%s{path}.%s{export.Name},@%d{export.Ordinal}"")"

            formatted.Replace(@"\", @"\\").Replace(".dll", ""))
        exports

let rec proxyDll (dll: string) =
    let windowsPath = Path.Join(@"C:\Windows\", dll)
    let system32Path = Path.Join(@"C:\Windows\System32", dll)
    let sysWow64Path = Path.Join(@"C:\Windows\SysWOW64", dll)
    let path = [ windowsPath; system32Path; sysWow64Path ] |> Seq.filter File.Exists
    let path = Seq.head path
    let file = File.OpenRead(path)
    let pe = PeNet.PeFile(file)
    let exports = pe.ExportedFunctions |> Seq.filter (fun f -> f.HasName)
    let exports = proxyReplace (exports, path) |> String.concat "\n"

    let result =
        "#include <Windows.h>\n\n"
        + "\n"
        + "BOOL APIENTRY DllMain( HMODULE hModule,\n"
        + "                       DWORD  ul_reason_for_call,\n"
        + "                       LPVOID lpReserved\n"
        + "                     )\n"
        + "{\n"
        + "    switch (ul_reason_for_call)\n"
        + "    {\n"
        + "    case DLL_PROCESS_ATTACH:\n"
        + "    case DLL_THREAD_ATTACH:\n"
        + "    case DLL_THREAD_DETACH:\n"
        + "    case DLL_PROCESS_DETACH:\n"
        + "        break;\n"
        + "    }\n"
        + "    return TRUE;\n"
        + "}\n\n"
        + $"{exports}\n"

    result
