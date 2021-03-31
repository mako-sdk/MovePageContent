# MovePageContent

## Introduction

This example demonstrates how to move content, in this case glyphs (text) from one area of the page to another. It calculates the move from two annotations, added with Acrobat's commenting tool, to identify source and target areas.

## Usage

```plain
Usage: MovePageContent.exe <path to PDF test folder>
Example: MovePageContent testfiles
Output is written to a 'Results' folder the test folder.
```

## Preparing the document for processing

Using Acrobat (or the Acrobat Reader, it too can add comments) or another PDF viewer/editor:

1. Draw a rectangular box around the area you want to move. Use the rectangular shape tool, nothing else.

2. Add "Source" as the comment text, to identify it.

3. Do the same for the target area, adding "Target" as the comment text. Don't worry about the size, the app only cares about the top-left corner.

4. Finally, add a rectangle around some text that will identify the page. Add "Key=xxxx" as the comment text, where xxxx is the text that must be found at that position.

The key page is important. Unless a page has the key text at that position on the page, it will be ignored.

## Techniques employed by this example

This example makes use of the (Mako SDK) simple example _FindTextInRect_ that extracts text from a given area of the page. Here it is refactored as a method. It is called to determine whether a page should be processed, based on the presence or absence of the key text.

_FindTextInRect_ demonstrates an important technique for establishing the chain of transforms (scaling and positioning instructions) that determine the final size and position on the page. The same technique is used by the utility for each node that falls in the source area, adding the additional move with `.postMul()` (post-multiply) to its `CTransformState`-derived transform. The result is that the content within the source area is moved to the target area.

## Notes

This is a simple prototype that could be easily extended to process XPS; exactly the same process would apply. It could also be made to process multiple move instructions, by adding an index number to the "Source" and "Target" comments, and collecting these into vector of `MoveSpec`s.
