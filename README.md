# XmlUpgrader .net standard
Easy to use .net standard library for upgrading xml documents to a new format. It supports:
* Renaming nodes, adding nodes, removing nodes or execute custom xml operations
* Set new default values
* Includes automated tests for your upgrade scripts to produce the correct format


## Usage of XmlFileUpgrader
```c#
var upgrader = new XmlFileUpgrader();

upgrader.AddRegistration(new Version(1, 0), xmlToUpgradeFilePath);
upgrader.AddRegistration(new Version(2, 0), xmlUpgradeReferencePath, typeof(ExampleConfigV2));

var result = upgrader.UpgradeXml(xmlToUpgradeFilePath);
```
## How to create an upgrade script for one version
