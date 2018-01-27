# XmlUpgrader 
Easy to use .net standard library for upgrading xml documents to a new format. It supports:
* Renaming nodes, adding nodes with default value, removing nodes
* Includes automated tests for your upgrade scripts to produce the correct format
* Doing nothing if the xml file is already up to date

## Usage of XmlFileUpgrader
```c#
var upgrader = new XmlFileUpgrader();

upgrader.AddRegistration(new Version(1, 0), xmlToUpgradeFilePath);
upgrader.AddRegistration(new Version(2, 0), xmlUpgradeReferencePath, typeof(ExampleXmlVersion2));

UpgradeResult result = upgrader.UpgradeXml(xmlToUpgradeFilePath);
// result.UpgradeNeeded -> true
// result.UpgradedFromVersion -> Version 1.0
// result.UpgradedToVersion -> Version 2.0
```
## Implement an upgrade plan
* Create a class which implements `IUpgradePlanProvider`
* Create an `UpgradePlan` and implement your xml modifications

Order of modifications: 
1. Delete 
1. Rename 
1. Add 
```c#
    class ExampleXmlVersion2 : IUpgradePlanProvider
    {
        public string TestString { get; set; }

        public int TestInteger { get; set; }

        public string AddedValue { get; set; }

        public UpgradePlan GetUpgradePlan()
        {
            dynamic addedSettings = new ExpandoObject();
            addedSettings.AddedValue = "I'm an added value!";  // default value
            var upgradePlan = new UpgradePlan();
            upgradePlan.AddElements(addedSettings);
            return upgradePlan;
        }
    }
```
### Add nodes
```c#
dynamic elementsToAdd = new ExpandoObject();
elementsToAdd.AddedStructure = new ExpandoObject();
elementsToAdd.AddedStructure.SettingOne = "works";
var upgradePlan = new UpgradePlan()
                        .AddElements(elementsToAdd);
```
### Remove nodes
```c#
var removeElements = new List<string>();
removeElements.Add("/ExampleString"); // XPath

var upgradePlan = new UpgradePlan()
                        .RemoveElements(removeElements);    
```
### Rename nodes
Moves /ExampleString to /TestStructure/MovedSetting
```c#
dynamic renameMap = new ExpandoObject();
renameMap.TestStructure = new ExpandoObject();
renameMap.TestStructure.MovedSetting = "/ExampleString"; 

```
## Integration test for your `XmlFileUpgrader`
```c#
[Fact]
public void DetailedRegistration()
{
    var upgrader = new XmlFileUpgrader();

    upgrader.AddRegistration(version_1, @"Examples\Xml\Config_v1.xml");
    upgrader.AddRegistration(version_2, @"Examples\Xml\Config_v2.xml", typeof(ExampleConfigV2));

    upgrader.Verify();
}
```
# Conventions
Xml file should have a version tag in its root node e.g
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration version="1.0">
  <TestString>HelloWorld</TestString>
  <TestInteger>123</TestInteger>
</configuration>
```
XmlUpgrader uses internally `System.Version`. Visit [MSDN](https://msdn.microsoft.com/en-us/en-en/library/system.version(v=vs.110).aspx) for information on string formatting.