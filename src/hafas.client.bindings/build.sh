#!/usr/bin/env bash
# todo: add to build.fsx

dotnet pack
rm -rf ~/local.packages/hafas.client.bindings/1.0.0/
rm -rf ~/.nuget/packages/hafas.client.bindings/1.0.0/
/usr/local/bin/nuget.exe add bin/Debug/hafas.client.bindings.1.0.0.nupkg -source ~/local.packages -expand
