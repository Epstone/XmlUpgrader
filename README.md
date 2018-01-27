# XmlUpgrader .net standard
Easy to use .net standard library for upgrading xml documents to a new format. It supports:
* Renaming nodes, adding nodes, removing nodes
* Set new default values
* Includes automated tests for your upgrade scripts to produce the correct format


## Usage of XmlFileUpgrader
```c#
var upgrader = new XmlFileUpgrader();

upgrader.AddRegistration(new Version(1, 0), xmlToUpgradeFilePath);
upgrader.AddRegistration(new Version(2, 0), xmlUpgradeReferencePath, typeof(ExampleConfigV2));

var result = upgrader.UpgradeXml(xmlToUpgradeFilePath);
```
## Implement an upgrade plan
* Create a class which implements `IUpgradePlanProvider`
* Create an `UpgradePlan` and implement your xml modifications
* Order of modifications: Delete, Rename, Add 
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
## Integration test for your `XmlFileUpgrader` instance
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
