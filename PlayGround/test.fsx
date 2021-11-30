let find f xs ys = 
    xs |> List.map (fun x -> (x, List.tryFind (f x) ys))
       |> List.iter (function (x, None) -> printfn "%A" x
                            | (x, Some y) -> printfn "%A,%A" x y)

let compare2Lists oper l1 l2 = 
        let mutable list = []  
        l1 
        |> List.map (fun x -> (x, List.tryFind (oper x) l2))
        |> List.iter (function (x, None) -> () | (x, Some y) -> list <- list@[y])
        list

compare2Lists (=) ["1";"2";"3";"4"] ["3";"1"] |> printfn "%A"