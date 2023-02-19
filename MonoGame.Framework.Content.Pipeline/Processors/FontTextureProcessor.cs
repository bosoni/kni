﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessorAttribute(DisplayName = "Font Texture - MonoGame")]
    public class FontTextureProcessor : ContentProcessor<Texture2DContent, SpriteFontContent>
    {
        private Color transparentPixel = Color.Magenta;

        [DefaultValue(' ')]
        public virtual char FirstCharacter { get; set; }

        [DefaultValue(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public FontTextureProcessor()
        {
            FirstCharacter = ' ';
            PremultiplyAlpha = true;
        }


        public override SpriteFontContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            var output = new SpriteFontContent();

            // extract the glyphs from the texture and map them to a list of characters.
            // we need to call GtCharacterForIndex for each glyph in the Texture to 
            // get the char for that glyph, by default we start at ' ' then '!' and then ASCII
            // after that.
            BitmapContent face = input.Faces[0][0];
            SurfaceFormat faceFormat;
            face.TryGetFormat(out faceFormat);
            if (faceFormat != SurfaceFormat.Color)
            {
                var colorFace = new PixelBitmapContent<Color>(face.Width, face.Height);
                BitmapContent.Copy(face, colorFace);
                face = colorFace;
            }

            var glyphs = ExtractGlyphs((PixelBitmapContent<Color>)face);
            // Optimize.
            foreach (var glyph in glyphs)
            {
                glyph.Crop();

                output.VerticalLineSpacing = Math.Max(output.VerticalLineSpacing, glyph.Subrect.Height);
            }

            // Get the platform specific texture profile.
            var texProfile = TextureProfile.ForPlatform(context.TargetPlatform);

            // We need to know how to pack the glyphs.
            bool requiresPot, requiresSquare;
            texProfile.Requirements(context, TextureFormat, out requiresPot, out requiresSquare);

            face = GlyphPacker.ArrangeGlyphs(glyphs, requiresPot, requiresSquare);

            foreach (Glyph glyph in glyphs)
            {
                output.CharacterMap.Add(GetCharacterForIndex((int)glyph.GlyphIndex));

                var texRect = glyph.Subrect;
                output.Glyphs.Add(texRect);

                Rectangle cropping;
                cropping.X = (int)glyph.XOffset;
                cropping.Y = (int)glyph.YOffset;
                cropping.Width = glyph.Width;
                cropping.Height = glyph.Height;
                output.Cropping.Add(cropping);

                output.Kerning.Add(glyph.Kerning.ToVector3());
            }

            output.Texture.Faces[0].Add(face);

            ProcessPremultiplyAlpha(face);

            // Perform the final texture conversion.
            texProfile.ConvertTexture(context, output.Texture, TextureFormat, true);

            return output;
        }


        protected virtual char GetCharacterForIndex(int index)
        {
            return (char)(((int)FirstCharacter) + index);
        }

        private List<Glyph> ExtractGlyphs(PixelBitmapContent<Color> bitmap)
        {
            var glyphs = new List<Glyph>();
            var regions = new List<Rectangle>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y) != transparentPixel)
                    {
                        // if we don't have a region that has this pixel already
                        var re = regions.Find(r =>
                        {
                            return r.Contains(x, y);
                        });
                        if (re == Rectangle.Empty)
                        {
                            // we have found the top, left of a image. 
                            // we now need to scan for the 'bounds'
                            int top = y;
                            int bottom = y;
                            int left = x;
                            int right = x;
                            while (bitmap.GetPixel(right, bottom) != transparentPixel)
                                right++;
                            while (bitmap.GetPixel(left, bottom) != transparentPixel)
                                bottom++;
                            // we got a glyph :)
                            regions.Add(new Rectangle(left, top, right - left, bottom - top));
                            x = right;
                        }
                        else
                        {
                            x += re.Width;
                        }
                    }
                }
            }

            for (int i = 0; i < regions.Count; i++)
            {
                var rect = regions[i];
                var newBitmap = new PixelBitmapContent<Color>(rect.Width, rect.Height);
                BitmapContent.Copy(bitmap, rect, newBitmap, new Rectangle(0, 0, rect.Width, rect.Height));
                var glyphData = new Glyph((uint)i, newBitmap);
                glyphData.Kerning.AdvanceWidth = glyphData.Bitmap.Width;
                glyphs.Add(glyphData);
                //newbitmap.Save (GetCharacterForIndex(i)+".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            return glyphs;
        }

        private void ProcessPremultiplyAlpha(BitmapContent bmp)
        {
            if (PremultiplyAlpha)
            {
                byte[] data = bmp.GetPixelData();
                for (int idx = 0; idx < data.Length; idx += 4)
                {
                    byte r = data[idx + 0];
                    byte g = data[idx + 1];
                    byte b = data[idx + 2];
                    byte a = data[idx + 3];
                    Color col = Color.FromNonPremultiplied(r, g, b, a);

                    data[idx + 0] = col.R;
                    data[idx + 1] = col.G;
                    data[idx + 2] = col.B;
                    data[idx + 3] = col.A;
                }
                bmp.SetPixelData(data);
            }
        }
    }
}
