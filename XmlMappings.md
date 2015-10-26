# XML mapping definitions #
First, let's have a look at sample XML mapping definition:

```
<?xml version="1.0" encoding="utf-8" ?>	
<otis-mapping xmlns="urn:otis-mapping-1.0">
    <class name="Otis.Tests.UserDTO, Otis.Tests" source="Otis.Tests.Entity.User, Otis.Tests" >
        <member name="Id" />
        <member name="Age" />
        <member name="UserName" expression="$UserName.ToUpper()" nullValue="[unknown]" />
        <member name="FullName" expression="[$FirstName + ' ' + $LastName]" />
        <member name="ProjectCount" expression="$Projects.Count" />
        <member name="Title" expression="$Gender" >
            <map from="Gender.Male" to="Mr." />     <!-- projections -->
            <map from="Gender.Female" to="Mrs." />
        </member> 
        <member name="Birthday"  expression="$BirthDate" format="Born on {0:D}"/>
        <member name="ProjectCount" expression="$Projects.Count" />
        <member name="AvgTaskDuration" expression="avg:$Projects/Tasks/Duration" />
        <member name="MaxTaskDuration" expression="max:$Projects/Tasks/Duration" />  		
    </class>
</otis-mapping>
```

Root element of the document is an `<otis-mapping>` element. Inside it, there is a `<class>` mapping, which defines a mapping from a source class to target class. There can be as many `<class>` elements inside an `<otis-mapping>` element as needed.

## Anatomy of `<class>` element ##
`<class>` element has two mandatory attributes:
`name` attribute holds a fully qualified name of the target class for transformation.
`source` attribute holds a fully qualified name of the source class.
Optionally, `helper` attribute can be used to specify the helper function for the transformation. Helper method can be a static method on any class with signature `static void HelperFunction(ref TargetClass t, ref SourceClass s)`, or an instance method on the target type with the same signature. Here is a sample of setting a helper function with `helper` attribute:
```
<class name="Otis.Tests.UserDTO, Otis.Tests" source="Otis.Tests.Entity.User, Otis.Tests" helper="Otis.Tests.Util.Convert" >
```

Inside the helper function, you can do any custom transformation, which is too complex to define in some other way. Example:
```
public class Util
{
	public static void Convert(ref UserDTO dto, ref User user)
	{
		dto.SomeComplexProperty = ... // do whatever you want here   
	}
}
```

Please note that `dto` instance has already been transformed according to mapping configuration, so you should only transform the members which can't be expressed with Otis.

`<class>` element can contain one `<member>` element for every property or field of the target type which must be mapped to some expression. For example, mapping `<member name="ProjectCount" expression="$Projects.Count" />` means that the ProjectCount property will be set to the value of the Count property of the Projects property of source class. In the code, it would look like this:
`target.ProjectCount = source.Projects.Count;`

There are four possible attributes for `<member>` element: `name`, `expression`, `nullValue` and `format`.

Only `name` attribute is mandatory and it specifies the name of the property or field on the target class which is being mapped. If the `expression` attribute is omitted, target member will be mapped to the source member with the same name. E.g. mapping `<member name="ID" />` will map ID property of the source type to the ID property of the target type (i.e. `target.ID = source.ID;`).

`expression` attribute specifies the expression which will be assigned to the property on target class specified by `name` attribute. This doesn't have to reference source members only. For example, mapping `<member name="AccessRights" expression="Security.GetDefaultRights()" />` is a valid mapping which will set AccessRights property of target type to the value returned by GetDefaultRights() call on Security static class.
If you need to use strings inside the expression, it can become awkward to use
double quotes which have to be escaped. In this case you can use simplified string expressions: enclose the expression in square brackets and use single quotes instead of double quotes. E.g., instead:
```
    <member name="FullName" expression="$FirstName + &quot; &quot; + $LastName" />
```
you can write:
```
    <member name="FullName" expression="[$FirstName + ' ' + $LastName]" />
```

You can also use aggregate expressions. These are functions which are calculated over collection expression to provide some result. E.g. following mapping calculates the average duration of all tasks in all projects of the source User object:
```
    <member name="AvgTaskDuration" expression="avg:$Projects/Tasks/Duration" />
```
You can find more details in MetadataMappings.

Optional attribute `nullValue` specifies the value which will be assigned to the target property if the the source expression equals null. E.g. mapping `<member name="UserID" expression="$UserName" nullValue="{UNKNOWN}" />` will set target.UserID to source.UserName, but if source.UserName is null, UserID will be set to "{UNKNOWN}".

`format` attribute specifies formatting string for the mapping. It only applies to the target members which are strings. E.g. mapping `<member name="Birthday" expression="$BirthDate" format="born on {0:D}" />` would be equal to the code expression:
`target.Birthday = string.Format("born on {0:D}", source.BirthDate);`

Each `<member>` element can contain `<map>` elements. These are used to provide projection mappings:
```
    <member name="Title" expression="$Gender" >
        <map from="Gender.Male" to="Mr." />     <!-- projections -->
        <map from="Gender.Female" to="Mrs." />
    </member> 
```

In the example above, we define mapping from User.Gender property
(of Gender enum type) to a string, so Gender.Male value will be converted to "Mr." string, while Gender.Female becomes "Mrs.". You can use projections to define mappings between any two sets of expressions.