# Introduction #

As explained in MappingNestedTargets, Otis can be used even when there is a significant mismatch between source and target object structures. However, syntax described there can be confusing because a separate mapping must be defined for each combination of mapped classes.

This documents discusses a better way of handling this situation. Currently, it is only a proposal, which will be refined, until it is usable enough to be implemented.

# Details #

One of the planned features for future versions is easier mapping of subcomponents. The idea is to provide something similar to `<component>` mapping in NHibernate.

First, let's have a look at the models being mapped. Suppose we have a following mapping which we would like to implement via Otis:

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

# Mapping #

## XML mapping ##

We want to describe this mapping via XML mapping file. A possible solution would look like this:

```
<?xml version="1.0" encoding="utf-8" ?>
<otis-mapping xmlns="urn:otis-mapping-1.0">
    <class name="Test.Domain.TargetRoot, Test.Domain" source="Test.Domain.SourceRoot, Test.Domain" >
        <member name="Id" />                       
        <member name="SubA" expression="source" /> 
        <nested name="TargetSubA" >
           <nested name="TargetSubB" > 
               <member name="Name" expression="$Sub1.Sub1Name" /> 
           </nested>
           <nested name="TargetSubC" > 
               <member name="Name" expression="$Sub2.Sub2Name" /> 
           </nested>
           <nested name="TargetSubD" > 
               <member name="RootName" expression="$Name" />
               <member name="SubName" expression="$Sub1.Sub1Name" /> 
           </nested>
        </nested>
    </class>
</otis-mapping>
```

## Metadata mapping ##

Attributes can also be used to define such mappings.

_todo_