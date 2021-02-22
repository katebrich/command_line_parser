# `CommandLineParser` API
## by Katerina Brichackova and David Novak

This is a C#/.NET API for parsing commands. Its strengths are extensibility, ease of use and clarity. The code is self-explanatory and well-commented, so there is no need for additional documentation. Firstly, take a look at the Examples directory to see the usage.

This project was created for university course **Recommended Programming Practices**. We worked in teams of two.
**The reviews and unit tests were written by other students!** In return, we created the tests and wrote the reviews for them.

The code should be easy to navigate, since all classes and methods are documented. In addition, a general scheme of functionality is included in *CommandLineParser/Examples/Program.cs* and a similar explanation of parsing methods can be found in *CommandLineParser/CommandLineParser/Parser.cs*. Graphical versions of these schemes are included in *api_presentation.pdf* and *progress_presentation.pdf*.

For the sake of consistency, all Regex strings used for matching during parsing are stored in *CommandLineParser/CommandLineParser/Properties/Resources.resx*, along with error message strings. This is useful if the client wishes to alter command syntax or create a localised version of the API.

The GitLab project includes a CI pipeline, consisting of a *build* and *test* stage. The *test* stage is made up of multiple scripts, each running its distinct subset of unit tests. The result of running each of these scripts can be obtained upon inspection.

