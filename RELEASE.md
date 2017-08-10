Make sure that the `TinCan/Properties/AssemblyInfo.cs` has been updated with new release version.

    TinCan -> Properties -> Assembly Information

Obtain the `TinCan.NET.pfx` file that is used for signing the relevant portions of the release.

Then right-click the solution in the "Solution Explorer" view and select "Batch Build...". Check the "Build" checkbox for the following `TinCan` configurations:
Release-net35
Release-net40
Release-net45
Release-net45-signed

Then click "Build" to build the selected configurations. (Verify `bin/Release/net<X>/TinCan.dll` has correct version.)

With `nuget.exe` installed and in your path do:

    cd TinCan
    nuget pack TinCan.csproj -sym -Prop Configuration=Release-net35
    nuget push TinCan.(version).nupkg

Note: Providing a `Configuration` property is mandatory, otherwise `nuget` will build the `Debug` configuration and include that in the package. The `<files>` portion of the `.nuspec` ensures all releases built previously are in the created `nuget` package.

Commit the updated assembly information file and push to master. Upload the generated `TinCan.(version).nupkg` as a GitHub tag release.
