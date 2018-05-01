﻿// *****************************************************************************
// 
//  © Component Factory Pty Ltd 2012. All rights reserved.
//	The software and associated documentation supplied hereunder are the 
//  proprietary information of Component Factory Pty Ltd, 17/267 Nepean Hwy, 
//  Seaford, Vic 3198, Australia and are supplied subject to licence terms.
// 
//  Version 4.4.0.2 	www.ComponentFactory.com
// *****************************************************************************

using System;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace ComponentFactory.Krypton.Toolkit
{
	/// <summary>
	/// Extends the professional renderer to provide Office2010 style additions.
	/// </summary>
    public class RenderOffice2013 : RenderOffice2010
    {
        #region Static Fields
        private static readonly float BORDER_PERCENT = 0.6f;
        private static readonly float WHITE_PERCENT = 0.4f;
        //private static readonly Blend _ribbonGroup5Blend;
        //private static readonly Blend _ribbonGroup6Blend;
        //private static readonly Blend _ribbonGroup7Blend;
        #endregion

        #region Identity
        static RenderOffice2013()
        {
            //_ribbonGroup5Blend = new Blend();
            //_ribbonGroup5Blend.Factors = new float[] { 0.0f, 0.0f, 1.0f };
            //_ribbonGroup5Blend.Positions = new float[] { 0.0f, 0.5f, 1.0f };

            //_ribbonGroup6Blend = new Blend();
            //_ribbonGroup6Blend.Factors = new float[] { 0.0f, 0.0f, 0.75f, 1.0f };
            //_ribbonGroup6Blend.Positions = new float[] { 0.0f, 0.1f, 0.45f, 1.0f };

            //_ribbonGroup7Blend = new Blend();
            //_ribbonGroup7Blend.Factors = new float[] { 0.0f, 1.0f, 1.0f, 0.0f };
            //_ribbonGroup7Blend.Positions = new float[] { 0.0f, 0.15f, 0.85f, 1.0f };
        }
        #endregion

        #region RenderRibbon Overrides
        /// <summary>
        /// Perform drawing of a ribbon cluster edge.
        /// </summary>
        /// <param name="shape">Ribbon shape.</param>
        /// <param name="context">Render context.</param>
        /// <param name="displayRect">Display area available for drawing.</param>
        /// <param name="paletteBack">Palette used for recovering drawing details.</param>
        /// <param name="state">State associated with rendering.</param>
        public override void DrawRibbonClusterEdge(PaletteRibbonShape shape,
                                                   RenderContext context,
                                                   Rectangle displayRect,
                                                   IPaletteBack paletteBack,
                                                   PaletteState state)
        {
            Debug.Assert(context != null);
            Debug.Assert(paletteBack != null);

            // Get the first border color
            Color borderColor = paletteBack.GetBackColor1(state);

            // We want to lighten it by merging with white
            Color lightColor = CommonHelper.MergeColors(borderColor, BORDER_PERCENT,
                                                        Color.White, WHITE_PERCENT);

            // Draw inside of the border edge in a lighter version of the border
            using (SolidBrush drawBrush = new SolidBrush(lightColor))
            {
                context.Graphics.FillRectangle(drawBrush, displayRect);
            }
        }

        #endregion

        #region IRenderer Overrides

        /// <summary>
        /// Gets a renderer for drawing the toolstrips.
        /// </summary>
        /// <param name="colorPalette">Color palette to use when rendering toolstrip.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public override ToolStripRenderer RenderToolStrip(IPalette colorPalette)
        {
            Debug.Assert(colorPalette != null);

            // Validate incoming parameter
            if (colorPalette == null)
            {
                throw new ArgumentNullException(nameof(colorPalette));
            }

            // Use the professional renderer but pull colors from the palette
            KryptonOffice2013Renderer renderer = new KryptonOffice2013Renderer(colorPalette.ColorTable)
            {

                // Seup the need to use rounded corners
                RoundedEdges = (colorPalette.ColorTable.UseRoundedEdges != InheritBool.False)
            };

            return renderer;
        }
        #endregion

        #region Implementation

        /// <summary>
        /// Internal rendering method.
        /// </summary>
        protected override IDisposable DrawRibbonTabContext(RenderContext context,
                                                            Rectangle rect,
                                                            IPaletteRibbonGeneral paletteGeneral,
                                                            IPaletteRibbonBack paletteBack,
                                                            IDisposable memento)
        {
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                Color c1 = paletteGeneral.GetRibbonTabSeparatorContextColor(PaletteState.Normal);
                Color c2 = paletteBack.GetRibbonBackColor5(PaletteState.ContextCheckedNormal);

                bool generate = true;
                MementoRibbonTabContextOffice2010 cache;

                // Access a cache instance and decide if cache resources need generating
                if ((memento == null) || !(memento is MementoRibbonTabContextOffice2010))
                {
                    if (memento != null)
                    {
                        memento.Dispose();
                    }

                    cache = new MementoRibbonTabContextOffice2010(rect, c1, c2);
                    memento = cache;
                }
                else
                {
                    cache = (MementoRibbonTabContextOffice2010)memento;
                    generate = !cache.UseCachedValues(rect, c1, c2);
                }

                // Do we need to generate the contents of the cache?
                if (generate)
                {
                    // Dispose of existing values
                    cache.Dispose();

                    cache.borderOuterPen = new Pen(c1);
                    cache.borderInnerPen = new Pen(CommonHelper.MergeColors(Color.Black, 0.1f, c2, 0.9f));
                    cache.topBrush = new SolidBrush(c2);
                    Color lightC2 = ControlPaint.Light(c2);
                    cache.bottomBrush = new LinearGradientBrush(new RectangleF(rect.X - 1, rect.Y, rect.Width + 2, rect.Height + 1),
                                                                Color.FromArgb(128, lightC2), Color.FromArgb(64, lightC2), 90f);
                }

                // Draw the left and right borders
                context.Graphics.DrawLine(cache.borderOuterPen, rect.X, rect.Y, rect.X, rect.Bottom);
                context.Graphics.DrawLine(cache.borderInnerPen, rect.X + 1, rect.Y, rect.X + 1, rect.Bottom - 1);
                context.Graphics.DrawLine(cache.borderOuterPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
                context.Graphics.DrawLine(cache.borderInnerPen, rect.Right - 2, rect.Y, rect.Right - 2, rect.Bottom - 1);
            
                // Draw the solid block of colour at the top
                context.Graphics.FillRectangle(cache.topBrush, rect.X + 2, rect.Y, rect.Width - 4, 4);

                // Draw the gradient to the bottom
                context.Graphics.FillRectangle(cache.bottomBrush, rect.X + 2, rect.Y + 4, rect.Width - 4, rect.Height - 4);
            }

            return memento;
        }
        /// <summary>
        /// Draw the application tab.
        /// </summary>
        /// <param name="shape">Ribbon shape.</param>
        /// <param name="context">Rendering context.</param>
        /// <param name="rect">Target rectangle.</param>
        /// <param name="state">State associated with rendering.</param>
        /// <param name="baseColor1">Base color1 used for drawing the ribbon tab.</param>
        /// <param name="baseColor2">Base color2 used for drawing the ribbon tab.</param>
        /// <param name="memento">Cached values to use when drawing.</param>
        public override IDisposable DrawRibbonApplicationTab(PaletteRibbonShape shape,
                                                             RenderContext context,
                                                             Rectangle rect,
                                                             PaletteState state,
                                                             Color baseColor1,
                                                             Color baseColor2,
                                                             IDisposable memento)
        {
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                bool generate = true;
                MementoRibbonAppTab2013 cache;

                // Access a cache instance and decide if cache resources need generating
                if ((memento == null) || !(memento is MementoRibbonAppTab2013))
                {
                    if (memento != null)
                    {
                        memento.Dispose();
                    }

                    cache = new MementoRibbonAppTab2013(rect, baseColor1);
                    memento = cache;
                }
                else
                {
                    cache = (MementoRibbonAppTab2013)memento;
                    generate = !cache.UseCachedValues(rect, baseColor1);
                }

                // Do we need to generate the contents of the cache?
                if (generate)
                {
                    // Dispose of existing values
                    cache.Dispose();

                    // Create common paths to all the app tab states
                    cache.GeneratePaths(rect, state);
                    //cache.borderPen = new Pen(baseColor1);

                    // Create state specific colors/brushes/pens
                    switch (state)
                    {
                        case PaletteState.Normal:
                            //cache.borderBrush = new SolidBrush(baseColor1);
                            cache.insideFillBrush = new SolidBrush(baseColor1);

                            //cache.insideFillBrush.SetSigmaBellShape(0.33f);
                            //cache.highlightBrush.CenterColor = Color.FromArgb(64, Color.White);
                            break;
                        case PaletteState.Tracking:
                            cache.insideFillBrush = new SolidBrush(baseColor2);
                            //cache.borderBrush = new SolidBrush(baseColor2);
                            //cache.insideFillBrush = new LinearGradientBrush(new RectangleF(rect.X, rect.Y + 1, rect.Width, rect.Height),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.3f, baseColor2, 0.7f),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.6f, baseColor2, 0.4f),
                            //                                                90f);

                            //cache.insideFillBrush.SetSigmaBellShape(0.33f);
                            //cache.highlightBrush.CenterColor = Color.FromArgb(100, Color.White);
                            break;
                        case PaletteState.Tracking | PaletteState.FocusOverride:
                            cache.insideFillBrush = new SolidBrush(ControlPaint.LightLight(baseColor2));
                            //cache.borderBrush = new SolidBrush(ControlPaint.LightLight(baseColor2));
                            //cache.insideFillBrush = new LinearGradientBrush(new RectangleF(rect.X, rect.Y + 1, rect.Width, rect.Height),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.3f, baseColor2, 0.7f),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.6f, baseColor2, 0.4f),
                            //                                                90f);

                            //cache.insideFillBrush.SetSigmaBellShape(0.33f);
                            //cache.highlightBrush.CenterColor = ControlPaint.LightLight(baseColor2);
                            break;
                        case PaletteState.Pressed:
                            cache.insideFillBrush = new SolidBrush(baseColor2);

                            //cache.borderBrush = new SolidBrush(CommonHelper.MergeColors(baseColor1, 0.5f, baseColor2, 0.5f));
                            //cache.insideFillBrush = new LinearGradientBrush(new RectangleF(rect.X, rect.Y + 1, rect.Width, rect.Height),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.3f, baseColor2, 0.7f),
                            //                                                CommonHelper.MergeColors(baseColor1, 0.75f, baseColor2, 0.25f),
                            //                                                90f);

                            //cache.insideFillBrush.SetSigmaBellShape(0f);
                            //cache.highlightBrush.CenterColor = Color.FromArgb(90, Color.White);
                            break;
                    }
                }

                // Fill the entire tab area and then add a border around the edge
                //context.Graphics.FillPath(cache.borderBrush, cache.borderFillPath);

                // Draw the outside border
                //using (AntiAlias aa = new AntiAlias(context.Graphics))
                //    context.Graphics.DrawPath(cache.borderPen, cache.borderPath);

                // Fill inside area
                //context.Graphics.FillPath(cache.insideFillBrush, cache.insideFillPath);
                context.Graphics.FillRectangle(cache.insideFillBrush, cache.rect);

                // Draw highlight over bottom half
                //using (Clipping clip = new Clipping(context.Graphics, cache.insideFillPath))
                //    context.Graphics.FillPath(cache.highlightBrush, cache.highlightPath);
            }

            return memento;
        }

        /// <summary>
        /// Internal rendering method.
        /// </summary>
        protected override IDisposable DrawRibbonTabSelected2010(RenderContext context,
                                                                Rectangle rect,
                                                                PaletteState state,
                                                                IPaletteRibbonBack palette,
                                                                VisualOrientation orientation,
                                                                IDisposable memento,
                                                                bool standard)
        {
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                Color c1 = palette.GetRibbonBackColor1(state);
                Color c2 = palette.GetRibbonBackColor2(state);
                Color c3 = palette.GetRibbonBackColor3(state);
                Color c4 = palette.GetRibbonBackColor4(state);
                Color c5 = palette.GetRibbonBackColor5(state);

                bool generate = true;
                MementoRibbonTabSelected2010 cache;

                // Access a cache instance and decide if cache resources need generating
                if ((memento == null) || !(memento is MementoRibbonTabSelected2010))
                {
                    if (memento != null)
                    {
                        memento.Dispose();
                    }

                    cache = new MementoRibbonTabSelected2010(rect, c1, c2, c3, c4, c5, orientation);
                    memento = cache;
                }
                else
                {
                    cache = (MementoRibbonTabSelected2010)memento;
                    generate = !cache.UseCachedValues(rect, c1, c2, c3, c4, c5, orientation);
                }

                // Do we need to generate the contents of the cache?
                if (generate)
                {
                    // Dispose of existing values
                    cache.Dispose();

                    // If we have a context color to use then modify the drawing colors
                    if (c5 != Color.Empty)
                    {
                        if (!standard)
                        {
                            c5 = CommonHelper.MergeColors(c5, 0.65f, Color.Black, 0.35f);
                        }

                        c1 = Color.FromArgb(196, c5);
                    }

                    switch (orientation)
                    {
                        case VisualOrientation.Top:
                            DrawRibbonTabSelectedTop2010(rect, c2, c3, c5, cache);
                            break;
                        case VisualOrientation.Left:
                            DrawRibbonTabSelectedLeft2010(rect, c2, c3, c5, cache);
                            break;
                        case VisualOrientation.Right:
                            DrawRibbonTabSelectedRight2010(rect, c2, c3, c5, cache);
                            break;
                        case VisualOrientation.Bottom:
                            DrawRibbonTabSelectedBottom2010(rect, c2, c3, c5, cache);
                            break;
                    }

                    cache.outsidePen = new Pen(c1);
                    cache.centerPen = new Pen(c4);
                }

                context.Graphics.FillRectangle(cache.centerBrush, cache.rect);
                //context.Graphics.FillPath(cache.centerBrush, cache.outsidePath);

                //if (c5 != Color.Empty)
                //    context.Graphics.FillPath(cache.insideBrush, cache.insidePath);

                //using (AntiAlias aa = new AntiAlias(context.Graphics))
                //    context.Graphics.DrawPath(cache.outsidePen, cache.outsidePath);
                context.Graphics.DrawRectangle(cache.outsidePen, cache.rect);

                //switch (orientation)
                //{
                //    case VisualOrientation.Top:
                //        DrawRibbonTabSelectedTopDraw2010(rect, cache, context.Graphics);
                //        break;
                //    case VisualOrientation.Left:
                //        DrawRibbonTabSelectedLeftDraw2010(rect, cache, context.Graphics);
                //        break;
                //    case VisualOrientation.Right:
                //        DrawRibbonTabSelectedRightDraw2010(rect, cache, context.Graphics);
                //        break;
                //    case VisualOrientation.Bottom:
                //        DrawRibbonTabSelectedBottomDraw2010(rect, cache, context.Graphics);
                //        break;
                //}
            }

            return memento;
        }

        /// <summary>
        /// Internal rendering method.
        /// </summary>
        protected override IDisposable DrawRibbonTabTracking2010(PaletteRibbonShape shape,
                                                                RenderContext context,
                                                                Rectangle rect,
                                                                PaletteState state,
                                                                IPaletteRibbonBack palette,
                                                                VisualOrientation orientation,
                                                                IDisposable memento,
                                                                bool standard)
        {
            if ((rect.Width > 0) && (rect.Height > 0))
            {
                Color c1 = palette.GetRibbonBackColor1(state);
                Color c2 = palette.GetRibbonBackColor2(state);
                Color c3 = palette.GetRibbonBackColor3(state);
                Color c4 = palette.GetRibbonBackColor4(state);
                Color c5 = palette.GetRibbonBackColor5(state);

                bool generate = true;
                MementoRibbonTabTracking2010 cache;

                // Access a cache instance and decide if cache resources need generating
                if ((memento == null) || !(memento is MementoRibbonTabTracking2010))
                {
                    if (memento != null)
                    {
                        memento.Dispose();
                    }

                    cache = new MementoRibbonTabTracking2010(rect, c1, c2, c3, c4, orientation);
                    memento = cache;
                }
                else
                {
                    cache = (MementoRibbonTabTracking2010)memento;
                    generate = !cache.UseCachedValues(rect, c1, c2, c3, c4, orientation);
                }

                // Do we need to generate the contents of the cache?
                if (generate)
                {
                    // Dispose of existing values
                    cache.Dispose();

                    // If c5 has a colour then use that to highlight the tab
                    if (c5 != Color.Empty)
                    {
                        if (!standard)
                        {
                            c5 = CommonHelper.MergeColors(c5, 0.65f, Color.Black, 0.35f);
                        }

                        c1 = c5;
                        c2 = CommonHelper.MergeColors(c2, 0.8f, ControlPaint.Light(c5), 0.2f);
                        c3 = CommonHelper.MergeColors(c3, 0.7f, c5, 0.3f);
                    }

                    switch (orientation)
                    {
                        case VisualOrientation.Top:
                            DrawRibbonTabTrackingTop2010(rect, c3, c4, cache);
                            break;
                        case VisualOrientation.Left:
                            DrawRibbonTabTrackingLeft2010(rect, c3, c4, cache);
                            break;
                        case VisualOrientation.Right:
                            DrawRibbonTabTrackingRight2010(rect, c3, c4, cache);
                            break;
                        case VisualOrientation.Bottom:
                            DrawRibbonTabTrackingBottom2010(rect, c3, c4, cache);
                            break;
                    }

                    cache.outsidePen = new Pen(c1);
                    cache.outsideBrush = new SolidBrush(c2);
                }

                // Fill the full background
                //context.Graphics.FillPath(cache.outsideBrush, cache.outsidePath);
                context.Graphics.FillRectangle(cache.outsideBrush,cache.rect);

                // Draw the border
                //using (AntiAlias aa = new AntiAlias(context.Graphics))
                //    context.Graphics.DrawPath(cache.outsidePen, cache.borderPath);
                context.Graphics.DrawRectangle(cache.outsidePen, cache.rect);

                // Fill the inside area
                //context.Graphics.FillPath(cache.insideBrush, cache.insidePath);
            }

            return memento;
        }
    }
    #endregion
}
