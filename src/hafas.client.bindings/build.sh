#!/usr/bin/env bash
# todo: add to build.fsx

VERSION="6.0.0"

dotnet pack
rm -rf ~/local.packages/hafas.client.bindings/${VERSION}/
rm -rf ~/.nuget/packages/hafas.client.bindings/${VERSION}/
/usr/local/bin/nuget.exe add bin/Debug/hafas.client.bindings.${VERSION}.nupkg -source ~/local.packages -expand
