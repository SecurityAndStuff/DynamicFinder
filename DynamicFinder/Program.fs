open System.Diagnostics
open System.IO
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
        printfn "Usage: %s [User|Administrators|System]" (Assembly.GetEntryAssembly().GetName().Name)
        exit (1)
    | _ -> args.[1].ToLower()


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

printfn "%s" $"[*] There are {Seq.length writableDirectories} directories from running applications we can write to."

let displayDlls path =
    let dlls = Dll.getDlls path

    if Seq.length dlls <> 0 then
        printfn "%s" $"[+] There are {Seq.length dlls} DLLs we can use for {path}"

    dlls |> Seq.iter (fun dll -> printfn "%s" ($"\t- {dll}"))

writableDirectories |> Seq.iter displayDlls
