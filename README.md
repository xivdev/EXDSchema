# EXDSchema
## Introduction
This is the schema repository for SqPack [Excel data](https://xiv.dev/game-data/file-formats/excel).

## Sheets
Inside SqPack, category 0A consists of Excel sheets serialized into a proprietary binary format read by the game.
The development cycle generates header files for each sheet, which are then compiled into the game, thus, all structure information
is lost on the client side when the game is compiled. This repository is an attempt to consolidate efforts into a
language agnostic schema, easily parsed into any language that wishes to consume it, that accurately describes the structure
of the EXH files as they are provided to the client.

## Schema
Schemas are written in YML to define the fields of the structure in an EXH file and the links between different fields.
The schema provides a number of features, all of which is enforced by the provided JSON schema for the schema. When applied
against an EXD schema file, it will provide IDE completion and error-checking to improve the manual editing experience.

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
- Example code
  - A C# project that perform the conversion from the 
    common JSON schemas ("SC Schema") to the EXDSchema format
  - A C# project that will validate EXDSchemas against a given game installation
  - A tool for editing schemas and viewing the results of parsing on-the-fly

## Usage
See Usage.md.