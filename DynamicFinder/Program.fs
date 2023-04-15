open System
open System.Diagnostics
open System.DirectoryServices.AccountManagement
open System.IO
open System.Management
open System.ServiceProcess
open System.Reflection

let printProcesses processes =
    processes
    |> Seq.map (fun (proc: Process) -> proc.ProcessName)
    |> Seq.iter (printfn "%s")

let printServices services =
    services
    |> Seq.map (fun (service: ServiceController) -> service.DisplayName)
    |> Seq.iter (printfn "%s")


let args = System.Environment.GetCommandLineArgs()

let user: string =
    match args.Length with
    | 0
    | 1 ->
        eprintfn $"Usage: %s{Assembly.GetEntryAssembly().GetName().Name} [User|Group]"
        exit 1
    | _ -> args[1].ToLower()

let wmiSearch (query: string) =
    let searcher = new ManagementObjectSearcher(query)
    let search = searcher.Get()

    seq
        [| for result in search ->

               result |]

let allUsers = wmiSearch "SELECT * FROM Win32_UserAccount"

let allGroups = wmiSearch "SELECT * FROM Win32_Group"

if isNull (Principal.FindByIdentity(new PrincipalContext(ContextType.Machine), user)) then
    eprintfn $"[*] '%s{args[1]}' was not found."
    printfn "[*] Possible values for users are:"

    for result in allUsers do
        printfn "\t- %s" (string result["Name"])

    printfn "[*] Possible values for groups are:"

    for result in allGroups do
        printfn "\t- %s" (string result["Name"])

    exit 1

printfn "[*] Running..."

let runningProcs = Proc.runningProcesses
let runningSvcs = Service.runningServices

let runningFilenames =
    (Seq.concat
        [ (runningProcs |> Seq.map Proc.ProcPath)
          (runningSvcs |> Seq.map Service.servicePath) ])
    |> Seq.distinct

let writableDirectories =
    runningFilenames
    |> Seq.filter (fun path -> Dll.canWriteToDirectory (Path.GetDirectoryName(path), user))

printfn $"[*] There are %d{Seq.length writableDirectories} directories from running applications we can write to."

let displayDlls path dlls =
    if Seq.length dlls <> 0 then
        printfn $"[+] There are %d{Seq.length dlls} DLLs we can use for %s{path}"

    dlls |> Seq.iter (fun dll -> printfn $"\t- %s{dll}")


let mutable createdProxies = [||]

let dlls = writableDirectories |> Seq.map Dll.getDlls

Seq.iter2 displayDlls writableDirectories dlls

let createProxy =
    if not (Directory.Exists("Output")) then
        Directory.CreateDirectory("Output") |> ignore

    for dllFiles in dlls do
        for dll in dllFiles do
            let filePath =
                Path.Join("Output", dll.Replace(".dll", ".c", StringComparison.OrdinalIgnoreCase))

            if not (createdProxies |> Seq.contains filePath) then
                let result = Dll.proxyDll dll
                File.WriteAllText(filePath, result)
                createdProxies <- Array.append createdProxies [| filePath |]
                printfn $"[*] Created proxy for %s{Path.GetFileName(filePath)}"
