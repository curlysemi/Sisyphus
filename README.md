# Sisyphus

The goal of this project is an executable that can be used to validate legacy .NET project files that often run into issues when working in a large team setting.

The three primary issues that arise in such scenarios (which are mostly due to merging) are:
* project files may mistakenly not have all of the files included that are tracked by git or that are on disk
* project files may mistakenly have duplicate includes for some files
* project files may get out-of-sync with the dependencies in `packages.config`

These problems are such that they are encountered during runtime, which isn't all that cool.

.NET Core solved most of these problems with support for globbing, but globbing doesn't work correctly with _legacy_ project formats. :'(

## Commands
```
  check      Check the provided solution or project file for missing includes.

  dedup      Remove duplicate file-references from project file or projects in solution file. Sorting first is
             recommended.

  sort       Sort the contents of the provided project file or projects in provided solution file.

  mkconf     Create a new config file if none exists or print out an existing config.

  verdep     Check the provided solution or project file for dependency conflicts.

  help       Display more information on a specific command.

  version    Display version information.
```

Using the `help` command, the options for a particular command can be shown:

```bash
$ sisyphus help check
```
```
sisyphus 0.0.1
Copyright (C) 2019 Web Team

  -i, --input      Input project file of solution file path.

  -v, --verbose    Run with verbose logging.

  -c, --config     Set the path for the 'Sisyphus.js' configuation file (commands that take an input path argument will
                   check for a nearby config file by default).

  --help           Display this help screen.

  --version        Display version information.
```