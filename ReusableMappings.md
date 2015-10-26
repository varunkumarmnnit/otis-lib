# Introduction #

This document discusses a way to reuse a set of mappings between classes which are not related via inheritance.

# Details #

We have following entities:

```
public class Entity1
{
   public string Entity1Id {get;set;}
   public string Type {get;set;}
}

public class Entity2
{
   public string Entity2Id {get;set;}
   public string Type {get;set;}
}
```

Please note that they are not related via common base class or any other way.

Now, we want to map these entities to following DTOs:

```
public enum RowType
{
   Golden,
   Silver,
   Standard
}

public class BaseDTO
{
   public RowType RowType {get;set;}
}

public class DerivedDTO1 : BaseDTO
{
   public string Derived1Id {get;set;}
}

public class DerivedDTO2 : BaseDTO
{
   public string Derived2Id {get;set;}
}
```

Otis should perform following mappings:
```
  Entity1.Entity1Id -> DerivedDTO1.Derived1Id 
  Entity1.Type      -> DerivedDTO1.RowType
  Entity2.Entity2Id -> DerivedDTO2.Derived2Id
  Entity2.Type      -> DerivedDTO2.RowType
```

Currently, you would have to define 2 separate mappings for this:
```
<class name="DerivedDTO1" source="Entity1">
        <member name="Derived1Id" expression="$Entity1Id"/>
        <member name="RowType" expression="$Type.ToLower()">
            <map from="['g']" to="RowType.Golden" />
            <map from="['s']" to="RowType.Silver" />
            <map from="['u']" to="RowType.Standard" />
        </member>
</class>

<class name="DerivedDTO2" source="Entity2">
        <member name="Derived2Id" expression="$Entity2Id"/>
        <member name="RowType" expression="$Type.ToLower()">
            <map from="['g']" to="RowType.Golden" />
            <map from="['s']" to="RowType.Silver" />
            <map from="['u']" to="RowType.Standard" />
        </member>
</class>
```

`Type -> RowType` is defined twice, once in each mapping. This doesn't sound that hard, but imagine if there was 5 mapped properties in `BaseDTO`. All 5 items would have to be defined in both mappings, which is tedious and error prone.

Therefore, there has to be a better way to do this. `<sharedMap>` element is introduced to define item mappings which are reused in other class mappings. Attribute `shared` is then used in class mappings to include the shared item mappings. An example will make it clearer:

```
<class name="DerivedDTO1" source="Entity1" shared="shared1">
        <member name="Derived1Id" expression="$Entity1Id"/>
</class>

<class name="DerivedDTO2" source="Entity2" shared="shared1">
        <member name="Derived2Id" expression="$Entity2Id"/>
</class>

<!-- sharedMap element defines item mappings to be shared -->
<sharedMap name="shared1">
        <member name="RowType" expression="$Type.ToLower()">
            <map from="['g']" to="RowType.Golden" />
            <map from="['s']" to="RowType.Silver" />
            <map from="['u']" to="RowType.Standard" />
        </member>
</sharedMap>
```

`shared` attributes can contain multiple shared mapping references: `shared="shared1,shared2"`

_todo: how to do this via attributes?_