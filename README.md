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

  -e, --errors     Consider any issues to be errors (non-zero return).

  -i, --input      Input project file of solution file path.

  -v, --verbose    Run with verbose logging.

  -c, --config     Set the path for the 'Sisyphus.js' configuration file (commands that take an input path argument will
                   check for a nearby config file by default).

  --help           Display this help screen.

  --version        Display version information.
```

## Setup
There's several different ways that `sisyphus` can be integrated with legacy-format .NET projects. Here are some ideas:
1. As a utility that is manually invoked. Either install locally and add to your PATH or add the EXE to repo.
3. As a utility that is invoked using the post-checkout git hook.
2. As a pre-build (or post-build) event.

## Examples
The following examples assume the directory containing `sisyphus.exe` has been added to your PATH. If this is not the case, substitute `sisyphus` with the path to your local `sisyphus.exe` file.
### Creating a `Sisyphus.js` config file
The `Sisyphus.js` config file contains settings for (i) excluding particular files from being checked by the `check` command (intended for development files that are tracked by version control but are not necessarily files that need be included in the project file) and (ii) a setting for the `RelativePackagesPath` (which is used by some of the options for the `verdep` command).

There are two ways to create a config file. (1) In the directory for the solution/project you wish to use with `sisyphus`:
```
$ sisyphus config
```
Or (2) explicitly provide the config file destination path:
```
$ sisyphus config -c C:\Users\SomeUser\Code\SomeProject
```

Either one of these approaches should result in the following output and a `Sisyphus.js` file:
```
Saving new config!
{
  "IgnorableFiles": [
    "**.gitignore",
    "**.tfignore",
    "**.exclude"
  ],
  "RelativePackagesPath": "..\\packages\\"
}
```

### Cleaning up project files
It is recommended to regularly "clean up" project files to make sure everything is as it should be.
Using `sisyphus` this should be a relatively easy process. This example assumes you are in the directory containing a solution file.
1. First, sort the items in the project file(s) alphabetically:
    ```
    $ sisyphus sort -i .\SomeGiantSolution.sln
    ```
    If you run `git status` at this point, you may see several changes. At this point, you may wish to see if your solution still builds. (It should.) Staging the changes at this point is recommended (`git add .`).

2. Then, remove any duplicate includes:
    ```
    $ sisyphus dedup -i .\SomeGiantSolution.sln
    ```
    If you see any changes when you `git status` at this point, then you had some duplicate includes. If you run `git diff`, you should be able to see them.

3. Then, check for any files that are missing from your project files:
    ```
    $ sisyphus check -e -i .\SomeGiantSolution.sln
    ```
    If there are any errors, you will see output similar to the following:
    ```
    'AProject' is not missing any files. :)

    BProject:
    (1)    BProject/Controllers/SomeController.cs
    (2)    BProject/Models/Some/SomeModel.cs

    CProject:
    (3)    CProject/Views/Home/Default.cshtml

    File(s) were missing from project(s).
    ```
    If there were _no_ errors, you would see something similar to the following:
    ```
    'AProject' is not missing any files. :)

    'BProject' is not missing any files. :)

    'CProject' is not missing any files. :)

    ```

4. Finally, you may wish to verify dependencies. This is achieved with the `verdep` command. This command has several options. Let's look at them:
    ```
    $ sisyphus help verdep
    ```
    ```
    sisyphus 0.0.1
    Copyright (C) 2019 Web Team

      -p, --hint-paths          Check for potential HintPath discrepancies.

      -f, --ignore-framework    When checking HintPath discrepancies, ignore differences in target frameworks.

      -d, --on-disk             When checking HintPath discrepancies, check that the packages are on disk.

      -e, --errors              Consider any issues to be errors (non-zero return).

      -i, --input               Input project file of solution file path.

      -v, --verbose             Run with verbose logging.

      -c, --config              Set the path for the 'Sisyphus.js' configuration file (commands that take an input path
                                argument will check for a nearby config file by default).

      --help                    Display this help screen.

      --version                 Display version information.
    ```
    This is one recommended command:
    ```
    $ sisyphus verdep -e -p -f -i .\SomeGiantSolution.sln
    ```
    You may see output like the following:
    ```
    AProject's 'CustomPackage':
    HP:     ..\packages\CustomLibrary.1.0.156\lib\net461\CustomPackage.dll
    GP:     ..\packages\CustomModel.1.0.0\lib\net462\CustomPackage.dll

    BProject's 'Microsoft.CSharp' has no hint path

    CProject's 'System.Net.Http' has no hint path

    Number of discrepancies:  1
    Number of no HintPaths:   2
    Number of fine HintPaths: 42

    ```
    ("HP" refers to the actual hint path and "GP" refers to the 'guessed hint path' using the packages.config file.)
