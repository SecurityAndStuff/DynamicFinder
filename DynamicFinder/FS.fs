module Dll

open System.IO
open Microsoft.Win32
open System.Security.AccessControl
open System.DirectoryServices.AccountManagement

let knownDlls =
    let keyName = @"SYSTEM\CurrentControlSet\Control\Session Manager\KnownDLLs"
    let key = (Registry.LocalMachine.OpenSubKey keyName)

    key.GetValueNames()
    |> Seq.map key.GetValue
    |> Seq.map (fun (dll) -> (string dll).ToLower())


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
            |> Seq.filter (fun dll -> not (System.IO.File.Exists($@"{Directory.GetDirectoryRoot(path)}\{dll}")))

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
    | :? System.UnauthorizedAccessException
    | :? System.InvalidCastException
    | :? System.ArgumentException as ex -> false
