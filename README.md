# B# - Block Sharp
> Inspired by the Boo programming language, Python Zen, F# and Glen Block's ScriptCS

Less typing, more action!

Make C# more wrist friendly by removing
- The need to semi-colon separate statements
- The need to type everything

... and adding
- Significant white-space

## Version 1
Syntax: `scriptcs block.csx` in the directory where your `.bs` file is.
... Will transpile your B# script into CSharp Script and then execute it using the ScriptCS runtime.

### Supported features
#### Auto string interpolation, semi-colon and parenthesis insertion
    var firstName = 'Banana-rama'
    Console.WriteLine 'Hello {firstName}!'
  
#### ... turns into
    var firstName = "Banana-rama";
    Console.WriteLine($"Hello {firstName}");
  
#### Auto blocks & javascript style _if undefined_
    if 1 < 3
      Console.WriteLine 'yep, it''s smaller'
  
    var x = SomeMethodCall();
  
    if x
      throw InvalidOperationException(x)
    
#### ... turns into
    if (1 < 3) {
      Console.WriteLine($"Yep it\'s smaller");
    }

    var x = SomeMethodCall();

    if (x != null) {
      throw new InvalidOperationException(x);
    }


Listening to [8-Bit Electro & Glitch Hop Mix 2016: Best of 8-Bit Electro Gaming Mix – Pixl Podcast Ep. 6](https://www.youtube.com/watch?v=kPL_pO7b8wA)
