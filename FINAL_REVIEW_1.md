# Final review

## Documentation

The documentation is in the form of `README.md` file and in the
`api_presentation.pdf` and `progress_presentation.pdf` presentations. The code
has great readability and it is mostly self-documenting. Further explanation
is provided where needed in documentation comments in the code. I especially
like the ASCII art diagrams included to explain the architecture.

All comments in the code that I've seen are all encompassing, written in full
sentences. I wish I have such a discipline to write comments like this. Well
done!

## Code

I like the way how a code in this repository is written. It is readable, it
does not violate conventions and it does exactly what I expect. It uses the
syntax sugar options of a new C# releases appropriately. The overall
implementation however does not rely on advanced concepts (like Attibutes).

I'd consider making the `IParameter` interface generic to fine-graine the types
of output parameters of the `TryParse` method. I however guess, that due to the
nature of a rest of the implementation the **team33** considered this and found
it inappropriate or impossible for some reason.


I am sad that obtaining the option argument is done via the option name passed
as string. I'm sure that there is a way how to do it less stringly typed
[number 7 here](https://blog.codinghorror.com/new-programming-jargon/).

```cis
var input = (DateTime)result.GetParameterValue("date");
```


## Tests

Tests use mainly the xUnit framework, although the VisualStudio testing
framework is also used in one project. I guess that this is the project from one
of the previous tasks. The test coverage is sufficient.

## Overall

Writing the example program was straightforward. I took inspiration in the
attached demonstration usage and in tests.

I find Your solution brilliant! The best I've seen here for sure.

What I consider as a huge mistake however is the absence of .gitignore file and
the presence of built artifacts in git repository. Also running the tests in CI
in parallel is a bit overkill here, as the whole repository is recompiled and
packages restored for each job again and again and the time penalty is way higher
than the time acquired by paralelization.
