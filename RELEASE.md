Make sure that the `TinCan/Properties/AssemblyInfo.cs` has been updated with new release version.

    TinCan -> Properties -> Assembly Information

Then set the Build Configuration to "Release" and build the solution. (Verify `bin/Release/TinCan.dll` has correct version.)

With `nuget.exe` installed and in your path do:

    cd TinCan
    nuget pack TinCan.csproj -Prop Configuration=Release
    nuget push TinCan.(version).nupkg

Commit the updated assembly information file and push to master. Upload the generated `TinCan.(version).nupkg` as a GitHub tag release.
