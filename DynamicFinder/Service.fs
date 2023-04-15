module Service

open System.Management

type Service =
    { Name: string
      Path: string
      StartName: string }

let runningServices =
    let query = "SELECT * FROM Win32_Service"
    let searcher = new ManagementObjectSearcher(query)

    let services =
        [ for service in searcher.Get() ->
              { Name = string service["Name"]
                Path = string service["PathName"]
                StartName = string service["StartName"] } ]

    services
    |> Seq.filter (fun svc -> Seq.length svc.Path <> 0 && not (svc.Path.Contains("svchost.exe -k")))

let serviceName (service: Service) = service.Name
let servicePath (service: Service) = service.Path
