# Change Log

## [1.0.9] - 08-01-2021
Changes
- Yet again I forgot to include the fixed files *grrr*. This release now totally contains the fixes promised in 1.0.8 :)

## [1.0.8] - 08-01-2021
Changes
- Resolved [Lua based plugins not working](https://github.com/Antix-Development/VSCode-Gideros-Debug/issues/4) issue.


## [1.0.7] - 07-01-2021
Changes
- Fixed *Sequence contains no elements* issue which was caused by attempts to read and analyze empty .lua files.

## [1.0.6] - 06-01-2021
Added
- Extension now knows to exclude .lua files where the first line in the file ends with the string *"!NOEXEC"*


## [1.0.5] - 06-01-2021
Changes
- Fixed the readme.


## [1.0.4] - 06-01-2021
Changes
- Updated readme to reflect the fact that manual gproj file name editing is no longer required.


## [1.0.3] - 06-01-2021
Changes
- Actually included the DLL file required to fix the "Access Denied" issue. Whoops!


## [1.0.2] - 06-01-2021
Fixes
- Resolved [Access Denied](https://github.com/Antix-Development/VSCode-Gideros-Debug/issues/1) on project launch issue.

Changes
- Replaced depreciated ${workspaceRoot} entries in launch.jsonwith ${workspaceFolder}. Thanks @keszegh
- Modified default launch.json configuration to use ${workspaceFolderBasename} for the gproj file name which negates the requirement to manually edit it. Thanks again @keszegh.


## [1.0.1] - 05-01-2021
Changes
- Updated readme


## [1.0.0] - 05-01-2021
- Initial release

Changes
- HandleOutput() in GiderosPlayerRemote changed to output *Info* instead of *PlayerOutput* because the latter does not seem to append a newline character when printing from Gideros.
