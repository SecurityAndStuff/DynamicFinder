module Proc

open System
open System.Diagnostics
open System.ComponentModel

let ProcPath (proc: Process) = proc.MainModule.FileName

let runningProcesses =
    Process.GetProcesses()
    |> Seq.filter (fun proc ->
        try

            not (isNull proc.MainModule.FileName)
        with
        | :? Win32Exception
        | :? InvalidOperationException -> false)
