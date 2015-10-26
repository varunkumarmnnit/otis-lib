# Mapping deeply nested targets to flat sources #

Very often, there is a significant mismatch between source and target object structures. In most cases, it is possible to handle such mappings without using helper methods.

Suppose we have a following mapping which we would like to implement via Otis:

```
TargetRoot        <- SourceRoot
  TargetSubA      <- [Nothing]
    TargetSubB    <- SourceRoot.SourceSub1
    TargetSubC    <- SourceRoot.SourceSub2
    TargetSubD    <- SourceRoot, SourceRoot.SourceSub1
```


Let's first have a look at target classes:
```
// NOTE: public fields are used only to make code shorter. In real life, 
// these would most probably be public properties, backed by private fields.                                     

namespace Test.Domain
{
  public class TargetRoot
  {
    public int Id;
    public TargetSubA SubA = new TargetSubA();
  }

  public class TargetSubA
  {
    public TargetSubB SubB = new TargetSubB();
    public TargetSubC SubC = new TargetSubC();
    public TargetSubD SubD = new TargetSubD();
  }

  public class TargetSubB
  {
    public string Name;
  }

  public class TargetSubC
  {
    public string Name;
  }

  public class TargetSubD
  {
    public string RootName;
    public string SubName;
  }
}
```

Now, here are the source classes:

```
namespace Test.Domain
{
  public class SourceRoot
  {
    public SourceSub1 Sub1 = new SourceSub1();
    public SourceSub2 Sub2 = new SourceSub2();
    public int Id = 99;
    public string Name = "Src Root Name";
  }

  public class SourceSub1
  {
    public string Sub1Name = "sub 1 name";
  }

  public class SourceSub2
  {
    public string Sub2Name = "sub 2 name";
  }
}
```

Now, we will create the mapping file.

NOTE: in mapping comments there are two types of descriptions:

| `<!-- TargetRoot.SubA <- SourceRoot -->` | SourceRoot object is mapped to TargetRoot.SubA field (of TargetSubA type). This will use the assembler, so we have to define that mapping, otherwise Otis will throw an OtisException with message "Assembler for transformation [!SourceRoot -> !TargetRoot.SubA ](.md) is not configured". |
|:-----------------------------------------|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

| `<!-- TargetSubD.RootName = SourceRoot.Name -->` | TargetSubD.RootName will be set to value of Name field of the source object (of type SourceRoot) |
|:-------------------------------------------------|:-------------------------------------------------------------------------------------------------|


```
<?xml version="1.0" encoding="utf-8" ?>
<otis-mapping xmlns="urn:otis-mapping-1.0">

    <class name="Test.Domain.TargetRoot, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="Id" />                                   <!-- TargetRoot.Id   =  SourceRoot.Id -->
        <member name="SubA" expression="source" />             <!-- TargetRoot.SubA <- SourceRoot -->
    </class>

    <class name="Test.Domain.TargetSubA, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="SubB" expression="$Sub1" />              <!-- TargetSubA.SubB <- SourceRoot.Sub1 -->
        <member name="SubC" expression="$Sub2" />              <!-- TargetSubA.SubC <- SourceRoot.Sub2 -->
        <member name="SubD" expression="source" />             <!-- TargetSubA.SubD <- SourceRoot -->
    </class>

    <class name="Test.Domain.TargetSubD, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="RootName" expression="$Name" />          <!-- TargetSubD.RootName = SourceRoot.Name -->
        <member name="SubName" expression="$Sub1.Sub1Name" />  <!-- TargetSubD.SubName  = SourceRoot.Sub1.Sub1Name -->
    </class>

    <class name="Test.Domain.TargetSubB, Test.Domain" source="Test.Domain.SourceSub1, Test.Domain" >
        <member name="Name" expression="$Sub1Name" />          <!-- TargetSubB.Name  = SourceSub1.Sub1Name -->
    </class>
	
    <class name="Test.Domain.TargetSubC, Test.Domain" source="Test.Domain.SourceSub2, Test.Domain" >
        <member name="Name" expression="$Sub2Name" />          <!-- TargetSubC.Name  = SourceSub2.Sub2Name -->
    </class>

</otis-mapping>
```

Most of this mapping is straight forward, but there is a trick which makes it possible to map target subobject to source root. In mapping:
```
    <class name="Test.Domain.TargetRoot, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="Id" />
        <member name="SubA" expression="source" />
    </class>
```

we define how SourceRoot is mapped to TargetRoot. Line `<member name="SubA" expression="source" />` defines that field SubA of class TargetRoot is mapped directly to the source object of the mapping (of SourceRoot type).

This means that when assemble comes to mapping TargetRoot.SubA to a SourceRoot instance, it will look for another mapping definition: SourceRoot -> TargetSubA, so we have to define that one also.

Now, in mapping SourceRoot to TargetSubA, we define that field TargetSubA.SubB retrieves the value from field SourceRoot.Sub1, and field TargetSubA.SubC retrieves the value from field SourceRoot.Sub2. Mappings between these classes are then defined in separate `<class>` elements (the last two in the xml sample).

Finally, we need to map TargetSubA.SubD: this one gets some values from SourceRoot and others from SourceSub1. Therefore, it is easiest to map it to topmost object, and reference subelements as needed:

```
    <class name="Test.Domain.TargetSubD, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="RootName" expression="$Name" />         <!-- TargetSubD.RootName = SourceRoot.Name -->
        <member name="SubName" expression="$Sub1.Sub1Name" /> <!-- TargetSubD.SubName  = SourceRoot.Sub1.Sub1Name -->
    </class>
```

### Next steps ###
One of the planned features for version 0.3 is easier mapping of subcomponents. The idea is to provide something similar to `<component>` mapping in NHibernate. A possible syntax
is described in NestedObjectMapping.