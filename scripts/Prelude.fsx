// use arg '--fsi-server-input-codepage:28591' for unicode characters
#r "nuget:FSharp.SystemTextJson";;
#r "nuget:Polyliner.Net";;
#r "nuget:FSlugify"
#r "../src/fshafas/bin/Debug/net5.0/fshafas.dll";;

open FsHafas
open FsHafas.Interactive;;

Serializer.addConverters ([|Serializer.UnionConverter<Client.ProductTypeMode>()|]);;

