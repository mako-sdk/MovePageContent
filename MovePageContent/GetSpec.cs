//  <copyright file="GetSpec.cs" company="Global Graphics Software Ltd">
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
    public class GetSpec
    {
        private readonly IJawsMako m_mako;

        public GetSpec(IJawsMako jawsMako)
        {
            m_mako = jawsMako;
        }

        public bool GetSpecFromPage(string docPath, uint pageIndex, out MoveSpecModel moveSpecOut)
        {
            using var page = IPDFInput.create(m_mako).open(docPath).getDocument(0).getPage(pageIndex);
            using var fixedPage = page.getContent();

            // Find the shape annotations on the page
            // Use their coordinates to determine the position for move specification
            var annotList = page.getAnnotations();
            if (!annotList.empty())
            { 
                var moveSpec = new MoveSpecModel();
                for (uint i = 0; i < annotList.size(); i++)
                {
                    if (annotList[i].getType() == IAnnotation.eAnnotationType.eATSquare)
                    {
                        var fieldText = annotList[i].getContents();
                        var widgetRect = annotList[i].getRect();
                        var fieldId = fieldText.Contains("=")
                            ? fieldText.Substring(0, fieldText.IndexOf("=", StringComparison.Ordinal)).ToLower().Trim()
                            : fieldText.ToLower().Trim();
                        switch (fieldId)
                        {
                            case "source":
                                moveSpec.Source = widgetRect;
                                break;

                            case "target":
                                moveSpec.Target = widgetRect;
                                break;

                            case "key":
                                moveSpec.KeyPos = widgetRect;
                                moveSpec.KeyMatch =
                                    fieldText.Substring(fieldText.IndexOf("=", StringComparison.Ordinal) + 1);
                                break;
                        }
                    }
                }

                moveSpecOut = moveSpec;
                return true;
            }

            moveSpecOut = null;
            return false;
        }
    }
}
