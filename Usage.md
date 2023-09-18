# Usage

## Initial Creation
To define a schema, you should create a file with the same name as the sheet it is defining.
The name field must contain the name of the sheet as well. If we were to write a schema for `AozActionTransient`, we would write the following in `AozActionTransient.yml`:

```yml
name: AozActionTransient
fields:
  - name: Field1
  - name: Field2
  - name: Field3
# etc ...
```

#### DisplayField
The `displayField` key is provided for consumers that wish to resolve a sheet reference within a single cell. It provides a hint
of what a user will *most likely* want to see when the current sheet is targeted by a link. For example, when linking to `BNpcName`,
the most likely column to reference would be `Name`. For `Item`, the most likely column might be `Name` or `Singular`.

## Defining Fields
All sheets must have a number of field entries that corresponds to the number of columns in that sheet.
If not, parsing should fail.

We can define fields like this:
```yml
type: sheet
fields:
  - name: Stats
  - name: Description
  - name: Icon
  - name: RequiredForQuest
  - name: PreviousQuest
  - name: Location
  - name: Number
  - name: LocationKey
  - name: CauseStun
  - name: CauseBlind
  - name: CauseInterrupt
  - name: CauseParalysis
  - name: TargetsSelfOrAlly
  - name: CauseSlow
  - name: TargetsEnemy
  - name: CausePetrify
  - name: CauseHeavy
  - name: CauseSleepy
  - name: CauseBind
  - name: CauseDeath
```
This schema is valid because it is accurate in structure. It defines a field for each column in the EXH file as of 6.48.

### Types
Valid types for fields in a schema are `scalar`, `array`, `icon`, `modelId`, and `color`.

#### scalar
The default type. If the `type` is omitted from a field, it will be assumed to be a `scalar`. Effectively does nothing except tell consumers that 
"this field is not an `array`".

#### icon : uint32
In the above AozActionTransient example,
```yml
  - name: Icon
```
can become
```yml
  - name: Icon
    type: icon
```
While this may seem redundant, there are many fields in sheets that refer to an icon within the `06`, or the `ui/` category,
but the field itself is just a `uint32`. This is a hint for any consumer that attempts to display this field that the data in this column
can be used to format an icon path, like generating `ui/icon/132000/132122_hr1.tex` when the field contains `132122`, without the consumer having
to manually determine which columns contain icons.

#### modelId : uint32, uint64
Model IDs in the game are packed into either a `uint32` or a `uint64`.

`uint32` packing is like so:
```
uint16 modelId
uint8 variantId
uint8 stain
```
`uint64` packing is like so:
```
uint16 skeletonId
uint16 modelId
uint16 variantId
uint16 stainId
```
To anyone *viewing* the data for research, the packed values are useless, so consumers that provide a view into sheet data can opt
to unpack these values and display them as their unpacked counterparts. Many tools utilize these values individually rather than packed,
so it's important to have the ability to define a field this way.

#### color : uint32
Some fields contain an RGB value for color in the ARGB format with no alpha. This is simply a hint if a consumer opts to display these
columns' fields as actual colors rather than the raw value.

#### array
Array fields provide the ability to group and repeat nested structures. For example, the notorious SpecialShop sheet:
```yml
name: SpecialShop
fields:
  - name: Name
  - name: Item
    type: array
    count: 60
    fields:
      - name: ReceiveCount
        type: array
        count: 2
      - name: CurrencyCost
        type: array
        count: 3
      - name: Item
        type: array
        count: 2
      - name: Category
        type: array
        count: 2
      - name: ItemCost
        type: array
        count: 3
      - name: Quest
        type: array
        count: 2
      - name: Unknown
      - name: AchievementUnlock
      - name: CollectabilityCost
        type: array
        count: 3
      - name: PatchNumber
      - name: HqCost
        type: array
        count: 3
      - type: array
        count: 3
      - name: ReceiveHq
        type: array
        count: 3
  - name: Quest
  - type: scalar
  - type: scalar
  - name: CompleteText
  - name: NotCompleteText
  - type: scalar
  - name: UseCurrencyType
  - type: scalar
  - type: scalar
```
As you can see, we have nested arrays in this structure. This means that the in-memory structure follows like so:
```C
struct SpecialShop
{
    struct
    {
        example_type ReceiveCount[2];
        example_type CurrencyCost[3];
        example_type Item[2];
        example_type Category[2];
        example_type ItemCost[3];
        example_type Quest[2];
        example_type Unknown;
        example_type AchievementUnlock;
        example_type CollectabilityCost[3];
        example_type PatchNumber;
        example_type HqCost[3];
        example_type Unknown2[2];
        example_type ReceiveHq[3];
    } Items[60];
    example_type Quest;
    example_type Unknown;
    example_type Unknown2;
    example_type CompleteText;
    example_type NotCompleteText;
    example_type Unknown3;
    example_type UseCurrencyType;
    example_type Unknown4;
    example_type Unknown5;
};
```
As you can see, the overall schema is similar to defining structures in YML but omitting the actual data type.
This nested capability allows you to define complex structures. However, to cut down on overall parsing complexity,
based on existing knowledge of the EXH data, **you may only nest twice.**

### Linking
The sheets that power the game are relational in nature, so the schema supports a few different kinds of linking.

#### Single Link
To define a link, simply add a link object:
```yml
  - name: Quest
    link:
      target: [Quest]
```
Note that the link target is an array of strings. They must be sheet names, and there must be at least one sheet. To link to one sheet, leave a single sheet in the array.

#### Multi Link
A sheet's single column can link to multiple columns:
```yml
  - name: Requirement
    link:
      target: [Quest, GrandCompany]
```
In this case, disparate sheet key ranges will provide the ability to determine which sheet a link should resolve to.
For example, if a row's `Requirement` is `2`, it will resolve to `GrandCompany`, because row `2` exists in `GrandCompany` and not in `Quest.`
The same thing happens in the other direction: if `Requirement` is `69208`, it will link to `Quest` and not `GrandCompany` for the same reason.

#### Conditional Link
A sheet's single column can link to multiple columns depending on another field in the sheet:
```yml
  - name: Location
    comment: PlaceName when LocationKey is 1, ContentFinderCondition when LocationKey is 4
    link:
      condition:
        switch: LocationKey
        cases:
          1: [PlaceName]
          4: [ContentFinderCondition]
```
The targets array is not required for conditional links.
When defining the link, add a `condition` object with a `switch` key that defines the field to switch on the value of.
The `cases` dictionary contains arrays of the sheet to reference when the case matches.

Yes, the `case` dictionary may contain an *array*. This means that each case can be a [multi link](#multi-link) as well. Take `Item` for example:
```yml
  - name: AdditionalData
    link:
      condition:
        switch: FilterGroup
        cases:
          14: [HousingExterior, HousingInterior,
               HousingYardObject, HousingFurniture,
               HousingFurniture, HousingPreset, 
               HousingUnitedExterior]
          15: [Stain]
          18: [TreasureHuntRank]
          20: [GardeningSeed]
          25: [AetherialWheel]
          26: [CompanyAction]
          27: [TripleTriadCard]
          28: [AirshipExplorationPart]
          32: [Orchestrion]
          36: [SubmarinePart]
```
The `AdditionalData` column does a lot of heavy lifting. We can assume during game execution that the use of the field is heavily based on context,
but for research and data exploration, having the ability to define the exact sheet is very useful. Here, we can see that when `FilterGroup` is `14`,
we can link to any of `HousingExterior`, `HousingInterior`, `HousingYardObject`, `HousingFurniture`, `HousingPreset`, or finally `HousingUnitedExterior`.
This works because the value for `AdditionalData` are distinct ranges, even when `FilterGroup` is `14`, thus allowing the definition here to behave like a multi link.