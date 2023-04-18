module Proc

open System
open System.Diagnostics
open System.ComponentModel
open System.Management

type Service =
    { Name: string
      Path: string
      }

let runningProcs =
    let query = "SELECT * FROM Win32_Process"
    let searcher = new ManagementObjectSearcher(query)

    let processes =
        [ for service in searcher.Get() ->
              { Name = string service["Name"]
                Path = string service["ExecutablePath"]
                } ]
    processes
