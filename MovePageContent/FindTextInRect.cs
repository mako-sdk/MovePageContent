//  <copyright file="FindTextInRect.cs" company="Global Graphics Software Ltd">
//      Copyright (c) 2021 Global Graphics Software Ltd. All rights reserved.
//  </copyright>
//  <summary>
//  This example is provided on an "as is" basis and without warranty of any kind.
//  Global Graphics Software Ltd. does not warrant or make any representations
//  regarding the use or results of use of this example.
//  </summary>
// -----------------------------------------------------------------------

using System;
using JawsMako;

namespace MovePageContent
{
    class FindTextInRect
    {
        public static string GetText(IDOMFixedPage content, FRect textRect)
        {
            // We will accumulate the text data in this string
            string str = null;

            // Now to find the text on the page; we could walk the DOM tree or use the
            // IDOMNode facilities to walk through the tree. Instead, let's use the
            // IDOMNode facilities to find all the IDOMGlyphs nodes on the page.
            // Note that we will not search inside brushes or IDOMForms.
            CEDLVectIDOMNode glyphsNodes = content.findChildrenOfType(eDOMNodeType.eDOMGlyphsNode);

            // For each glyphs node we found...
            foreach (var node in glyphsNodes.toArray())
            {
                // Get the glyphs node as glyphs
                IDOMGlyphs glyphs = IDOMGlyphs.fromRCObject(node);

                // Get the bounds of the glyphs node in the coordinate space
                // local to the glyphs node.
                FRect bounds = glyphs.getBounds(false);

                // Now this is the bounds in the coordinate space local to the glyphs
                // node itself. We now need to determine the bounds in the page coordinate
                // system. To do this, enlist the help of the CTransformState object which
                // when given a node, will (amongst other things) determine the complete
                // transformation matrix that is applied inside the glyphs node by tracing
                // the chain of parent nodes to the page level and concatenating transforms.
                CTransformState stateInGlyph = new CTransformState(glyphs);

                // Therefore, this the complete transform affecting the glyphs
                FMatrix glyphsTransform = stateInGlyph.transform;

                // Apply this transform to the glyphs bounds to determine the actual
                // conservative bounds of the glyphs node in the page coordinate space.
                glyphsTransform.transformRect(bounds);

                // Does this bounds intersect with the textRect?
                if (bounds.intersectsWithRect(textRect))
                {
                    // Ok - it touches - is it completely inside?
                    if (textRect.containsRect(bounds))
                    {
                        // Yes - take the entire string
                        str += glyphs.getUnicodeStringAsSysString();
                    }
                    else
                    {
                        // There are less verbose ways of doing this, but for clarity make this
                        // explicit. Some glyphs nodes may contain very long runs of text and
                        // here we are only looking for the text that is inside the search
                        // box.
                        //
                        // Here JawsMako provides a way to split a glyphs node into a series
                        // of glyphs nodes consisting of the smallest units of text possible.
                        // This will result in either an IDOMGlyphs node (if the glyphs node
                        // could not be broken down further), or some flavour of IDOMGroup
                        // whose children consist of glyphs nodes. For the former case we will
                        // just take the text. For the latter we need to inspect each of the
                        // glyphs.
                        //
                        // So, lets do that.

                        // First perform the split
                        IDOMNode split = glyphs.split();

                        // Is it still a single glyphs node?
                        if (split.getNodeType() == eDOMNodeType.eDOMGlyphsNode)
                        {
                            // The glyphs node cannot be broken down.
                            // Well, it isn't completely inside the rectangle.
                            // We will skip this.
                        }
                        else
                        {
                            // We expect a group to have been produced
                            IDOMGroup splitGroup = IDOMGroup.fromRCObject(split);
                            if (splitGroup == null)
                            {
                                throw new Exception("Expected a flavour of group here");
                            }

                            // Note: IDOMGlyphs.split() guarantees that the group will take on
                            // the transform of the glyphs node, with all the child glyphs having
                            // an identity transform. This means that the glyphsTransform
                            // determined previously is still valid for all the child glyphs.

                            // We expect a number of children, all glyphs nodes.
                            IDOMNode child = splitGroup.getFirstChild();
                            while (child != null)
                            {
                                IDOMGlyphs splitGlyphs = IDOMGlyphs.fromRCObject(child);
                                if (splitGlyphs == null)
                                {
                                    throw new Exception("Expected a glyphs node here");
                                }

                                // Obtain the bounds and transform
                                FRect splitBounds = splitGlyphs.getBounds(false);
                                glyphsTransform.transformRect(splitBounds);

                                // And if this is entirely inside the rect
                                if (textRect.containsRect(splitBounds))
                                {
                                    // We'll take the text
                                    str += splitGlyphs.getUnicodeStringAsSysString();
                                }

                                // And move to the next child
                                child = child.getNextSibling();
                            }

                        }
                    }

                }
            }
            return str;
        }
    }
}
