# Sisyphus

The goal of this project is an executable that can be used to validate legacy .NET project files that often run into issues when working in a large team setting.

The three primary issues that arise in such scenarios (which are mostly due to merging) are:
* project files may mistakenly not have all of the files included that are tracked by git or that are on disk
* project files may mistakenly have duplicate includes for some files
* project files may get out-of-sync with the dependencies in `packages.config`

These problems are such that they are encountered during runtime, which isn't all that cool.

.NET Core solved most of these problems with support for globbing, but globbing doesn't work correctly with _legacy_ project formats. :'(
