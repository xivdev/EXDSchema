# EXDSchema
<a href="https://thaliak.xiv.dev/repository/4e9a232b"><img align="right" src="https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fexd.camora.dev%2Fapi%2Fversions&query=latest&style=for-the-badge&label=Latest%20XIV%20Version"></a>

This is the schema repository for FFXIV's [Excel files](https://xiv.dev/game-data/file-formats/excel). It's recommended to use [EXDViewer](https://exd.camora.dev) for exploring this data and to help with providing contributions to EXDSchema.

## What is EXDSchema?
Inside SqPack (FFXIV's internal data archive format), category 0A (0a0000.win32... files) consists of "Excel" sheets (unrelated to the Microsoft Office program) serialized into a proprietary binary format read by the game. FFXIV's internal development cycle generates Excel header (.exh) files for each sheet, which are then compiled into the game, thus, all structure information is lost on the client side when the game is compiled.

This repository exists to consolidate efforts into a language agnostic schema, easily parsed into any language that wishes to consume it, that accurately describes the structure of the EXH files as they are provided to the client. Excel files (of which .exd files contain the actual data) are a core part of Final Fantasy XIV's data storage, containing relational/tabular information such as quests, items, and more. They're often used by the FFXIV community for datamining and developing community tools. Programmatic access to these files is typically done through via [Lumina](https://github.com/NotAdam/Lumina) (C#), [ironworks](https://github.com/ackwell/ironworks) (Rust), or [XIVAPI](https://xivapi.com/) (REST API).

## Schema
Schemas are written in [YAML](https://yaml.org) to define the fields of the structure in an EXH file and the links between different fields.
The schema provides a number of features, all of which is enforced by the [provided JSON schema](/schema.json) for the schema. When applied
against an EXDSchema file, it will provide IDE completion and error-checking to improve the manual editing experience.

## Features
Since EXH files define the data types for each column, the schema does not care or define any data types in the standard sense.
Instead, it focuses on declaratively defining the structure of the compiled EXH data structures and the relationships between fields.

The schema includes the following:
- Full declaration of fields is required, nothing can be omitted
- Support for a few common types across sheets, such as `modelId`, `color`, and `icon`
  - While these do not affect the overall parsing, they are
    useful for research as they provide an important hint for the purpose of the data
- Field names
- Arrays
- Links between fields
- Multi-targeting another sheet
- Complex linking between fields based on a `switch` conditional
- Comment support on any schema object
- Maps out-of-the-box to a very simple object mapping
- JSON schema for the schema itself, providing IDE completion and error-checking
- Relations to [group identically sized arrays together](https://en.wikipedia.org/wiki/AoS_and_SoA)

This repository hosts the schema files for each game version. Each change in the game's schema headers produces a new branch in this repository under `ver/202x.xx.xx.etc`. The `latest` branch will always contain the most up-to-date schemas. To view all the existing versions, see the [/schemas](/schemas) directory.

## Contributing

If you spot any changes you wish to make to a schema, make a pull request for the `latest` branch with your changes. To view your changes in realtime before making a pull request, use [EXDViewer](https://exd.camora.dev) and edit the schemas however you like. After making your pull request, Github Actions will automagically notify you of any possible errors you have in your schemas. If you wish to make a schema change for an older game version (and the sheet has been updated since then), make a pull request for the `ver/...` branch you wish to update.

## Code
EXDSchema maintains its versioning and consistency through Github Actions, and the PR process is automated to allow contributors to easily spot irregularities with their contributions. The following repositories are used to create and maintain EXDSchema:
- [EXDTools](https://github.com/xivdev/EXDTools): Contains all tooling required to maintain EXDSchema's CI/CD pipelines
- [EXDViewer](https://github.com/WorkingRobot/EXDViewer): Provides contributors a web and native tool with tight integration with [EXDSchema](https://github.com/xivdev/EXDSchema) for enhanced Excel data exploration and dynamic in-viewer schema editing
- [ffxiv-downloader](https://github.com/WorkingRobot/ffxiv-downloader): Allows EXDTools to manage and download FFXIV from a Github Action

## Usage
See [Usage.md](/Usage.md).
