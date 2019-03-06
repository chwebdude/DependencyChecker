# NuGet Dependency Checker

[![Build status](https://dev.azure.com/webdude/DependencyChecker/_apis/build/status/DependencyChecker)](https://dev.azure.com/webdude/DependencyChecker/_build/latest?definitionId=22)

Simple Tool for checking NuGet Dependencies and publish a report as build artifact *(no dashboard widget at the moment!)*.

### Configuration
![Overview](https://raw.githubusercontent.com/chwebdude/DependencyChecker/master/images/overview.jpg)

### Report
![Report](https://raw.githubusercontent.com/chwebdude/DependencyChecker/master/images/report.jpg)

## How to start

First add the root directory of the project as Searchpath. This is usually `$(Build.SourcesDirectory)`. If you also check the Search Recursive box all packages.config files will be analyzed.
Now you can say if you want to create a badge and if to create a report. The destination or the reportfile is very important. By default it will be created in the `$(Build.ArtifactStagingDirectory)` directory. Another build tasks has to handle this file. Ex. copy it to a webserver. Depending on your infrastructure you can also do this directly by `\\yourserver\yourshare\dependencies.html`. Just do the same with the badge path.

## Options

### Search Path

Path to check for the packages.config files.

### Search Recursive

Search subdirectories for the packages files.

### Create Badge

Create a badge using [DotBadge](https://github.com/rebornix/DotBadge).

### Badge Path

Where to save the badge and its name.

### Style

The style of the badge.

### Create Report

Create a report of the packages.

### Report Path

Location and name where to save report.
