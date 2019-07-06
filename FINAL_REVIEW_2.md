# FINAL CODE REVIEW

## Extension Program

The extension program was relatively easy to implement, but I still (as I mentioned in the API review) do not like the way typing is handled in the library.

The ease of implemenation was mostly due to standard .NET type `DateTime` doing almost all of the work for me. Two of the biggest flaws IMO are:
+ Users need to manually convert option parameter values, because they are always stored as objects. This is also the case when implement custom `IParameter` types   (see `DateParameter` using `out object result` requires conversion).
+ Options (if parsed, parameter values) are accessed by their option name. This requires either to store the options name in a variable or type them again where IDEs won't help you.

## Stability & Features

Library seems to be working fine, I have not encountered any bugs. I really like the nice, clean and familiar help printout the library produces. 

## Source Code

Code style is nice and clean, no major reservations from my side. Reasonable naming and formatting conventions commonly used in .NET are being consistently followed. 

I find the OO design reasonable, mostly following S.O.L.I.D. principles (have not found any major sins). 

Code is separated into methods well, which makes them quite easy to understand. Only flaw I found is unncessary use of `ref` parameters in methods, where the type of paramater being passed is already a reference type (any `class` in C#). The parameter is not (and should not be) modified within the method, therefore the `ref` keyword is redundant.

Regex parsing is an elegant solution. It could turn out to be a bit difficult to maintain if any bugs are discovered, but when it works, it is a really nice way (and probably quite fast as well using .NET standard library `Regex`) of parsing arguments.

Storing all the strings in resource files is probably a good idea. It should make possible future localization easier (although command-line tools are probably mostly fine being English-only :).

Despite some criticism, I find the code overall quite simple and elegant. It does not introduce much unnecessary complexity.

## Verdict

Although I have some reservations towards the API (which I voiced in the API review for this library), I rate the implementation as very good. 
