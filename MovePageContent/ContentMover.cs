//  <copyright file="ContentMover.cs" company="Global Graphics Software Ltd">
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
    public class ContentMover
    {
        private readonly IJawsMako m_mako;

        public ContentMover(IJawsMako jawsMako)
        {
            m_mako = jawsMako;
        }

        public bool MoveContent(string docPath, string outDocPath, MoveSpecModel moveSpec)
        {
            using var assembly = IPDFInput.create(m_mako).open(docPath);
            for (uint docIndex = 0; docIndex < assembly.getNumDocuments(); docIndex++)
            {
                var document = assembly.getDocument(docIndex);
                for (uint pageIndex = 0; pageIndex < document.getNumPages(); pageIndex++)
                {
                    // To determine if this is a page to be processed, we test the text in the 'key' area
                    var pageIdText = FindTextInRect.GetText(document.getPage(pageIndex).getContent(), moveSpec.KeyPos);
                    if (pageIdText == moveSpec.KeyMatch)
                    {
                        // Get the page, this time to edit
                        var fixedPage = document.getPage(pageIndex).edit();

                        // Fetch all the glyphsnodes; for this example, we only care about those
                        CEDLVectIDOMNode glyphsNodes = fixedPage.findChildrenOfType(eDOMNodeType.eDOMGlyphsNode);

                        // Create a matrix that shifts content from the source to the target area
                        var moveMatrix = new FMatrix(1.0, 0.0, 0.0, 1.0, moveSpec.Target.x - moveSpec.Source.x, moveSpec.Target.y - moveSpec.Source.y);

                        // For each glyphs node we found...
                        foreach (var node in glyphsNodes.toArray())
                        {
                            // This is code borrowed from the FindTextInRect class; see that for a detailed description
                            IDOMGlyphs glyphs = IDOMGlyphs.fromRCObject(node);
                            FRect bounds = glyphs.getBounds(false);
                            CTransformState stateInGlyph = new CTransformState(glyphs);
                            FMatrix glyphsTransform = stateInGlyph.transform;
                            glyphsTransform.transformRect(bounds);

                            // Is it completely inside the source area?
                            if (moveSpec.Source.containsRect(bounds))
                            {
                                // Add the additional move to the existing transform
                                glyphsTransform.postMul(moveMatrix);

                                // Apply to the glyphs node
                                glyphs.setRenderTransform(glyphsTransform);

                                // Remove from the page
                                glyphs.getParentNode().extractChild(glyphs);

                                // And add to the page - this ensures it is at top of Z-order
                                fixedPage.appendChild(glyphs);
                            }
                        }
                    }
                }
            }

            try
            {
                // Now we can write this to an PDF
                using var output = IPDFOutput.create(m_mako);
                output.writeAssembly(assembly, outDocPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown while writing PDF {outDocPath}: {e}");
                return false;
            }
        }
    }
}
