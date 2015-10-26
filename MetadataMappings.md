# Mapping definitions via attributes #
Let's have a look at a class mapping done via attributes

```
    // source class
    public class User
    {
        public int Id { get; }
        public int Age { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Address { get; }
        public DateTime BirthDate;
        public IList<Project> ProjectList;
        public Gender Gender; // Gender is an enum with values Male, Female
    }
    
    // target class
    [MapClass(typeof(User))]
    public class UserDTO
    {
        [Map("$Id")]   
        public int Id {get; }

        [Map]       
        public int Age {get; }

        [Map("\"<NotAvailable>\"")] 
        public string UserName {get; }

        [Map("[$FirstName + ' ' + $LastName]")] // uses simplified string (enclosed in [])
        public string FullName {get; }

        [Map(NullValue="\"<UNKNOWN>\"")] 
        public string Address {get; }

        [Map("$BirthDate", Format="born on {0:D}")]
        public string BirthDay {get; }

        [Map("$ProjectList")]
        public ProjectDTO[] Projects {get; }

        [Map("avg:$ProjectList/Tasks/Duration")]
        public double AverageTaskDuration {get; }

        [Map("avg:$Projects/Tasks/Duration")]
        public double AvgTaskDuration { get...} 

        [Map("$Gender", Projection = "Gender.Male => Mr.; Gender.Female => Mrs.")]
        public string Title { get ... }
    }
```

Mapping above defines the transformation from `User` class to `UserDTO` class. This is specified with `[MapClass(typeof(User))]` attribute on `UserDTO` class. `[MapClass]` attribute is initialized with the type of the source class.

It is also possible to specify helper function like this:
```
    [MapClass(typeof(User), Helper = "Otis.Tests.Util.Convert")]
    public class UserDTO {...}
```

For more details on helper functions, see [XmlMappings](XmlMappings.md).

Member mappings are defined via `[Map]` attribute. This attribute is allowed on properties as well as on fields, as long as they are public. Please note that the public fields are usually frowned upon, but feel free to choose for yourself.

The simplest form of `[Map]` attribute is without any parameters (example: `UserDTO.Age` property). This means that the member will be mapped to the source member with the same name (in this case `User.Age`).

This is usually not enough, so it is possible to define the source expression like this:
```
    [Map("[$FirstName + ' ' + $LastName]")] 
    public string FullName {get; }
```

This will map `UserDTO.FullName` to the code expression `source.FullName + " " + source.LastName`. '$' in mapping expression means that it is a reference to the source object, and in the assembler code it will be replaced with the source reference, thus resulting in the stated code expression. You can also see one additional feature here: simplified string mappings. When the mapping expression is closed in square brackets, then the single quote character will be treated as double quote character (which needs escaping inside a string). Without this the expression would be `"$FirstName + \" \" + $LastName"`. Of course, you can use that if you like it.

On the other hand, if the expression doesn't reference the source object, it is written as is:
```
    [Map("\"<NotAvailable>\"")] 
    public string UserName {get; }
```
This is equivalent to code expression `target.UserName = "<NotAvailable>"`.

If null values of the source expression needs special handling, it is possible to provide null replacement value via `NullValue` property, like this:
```
    [Map(NullValue="\"<UNKNOWN>\"")] 
    public string Address {get; }
```
In this case, if `source.Address` is null, `Address` property of the target object will be set to "

&lt;UNKNOWN&gt;

".

It is also possible to format the source expression, like in this mapping:
```
    [Map("$BirthDate", Format="born on {0:D}")]
    public string BirthDay {get; }
```
This would be equal to the code expression:
`target.BirthDay = string.Format("born on {0:D}", source.BirthDate);`

Otis automatically takes care of converting between different collection types:
```
    [Map("$ProjectList")]
    public ProjectDTO[] Projects {get; }
```
Here, `ProjectList` property which is an `IList<Project>` is converted to an array of `ProjectDTO`. Otis transparently creates an array and then transforms each `Project` into a `ProjectDTO` instance, which is then put into the array. Of course, conversion can be done the other way round: from an array to a list.

A very convenient feature of Otis library are aggregate functions. These are functions which are calculated over collection expression to provide some result. E.g. following mapping calculates the average duration of all tasks in all projects of the source User object:
```
    // User class has ProjectList property of type IList<Project>.
    // Project class has Tasks property of type IList<Task>.
    // Task class has an integer property named Duration.
    [Map("avg:$ProjectList/Tasks/Duration")]
    public double AverageTaskDuration {get; }
```

Following aggregate functions are implemented by default:
| **Function** | **Description** |
|:-------------|:----------------|
| min          | returns the element with the smallest value |
| max          | returns the element with the greatest value |
| sum          | returns the sum of all element values |
| avg          | returns the average of all element values |
| count        | returns the count of all elements  |
| concat       | returns the string consisting of concatenated source elements (added in [version 0.2.17 ](http://code.google.com/p/otis-lib/downloads/list)) |

It is also possible to provide custom user-defined aggregate functions (see MedianFn in unit test library).

Projections were added in version 0.2. Projection mapping maps a set of values to another set of values. E.g.:
```
    [Map("$Gender", Projection = "Gender.Male => Mr.; Gender.Female => Mrs.")]
    public string Title { get ... }
```
This mapping will map Gender.Male value of User.Gender property to "Mr." value for Title property of UserDTO class. The format of the projection string is "SOURCE => TARGET". Multiple projections are separated by semicolons. You can also use simplified string expressions if you deal with strings.