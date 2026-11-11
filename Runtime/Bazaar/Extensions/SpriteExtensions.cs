using System;
using UnityEngine;
using System.Collections.Generic;

namespace Bazaar
{
    /// <summary>
    /// A set of extension methods for <c>Sprite</c> to manipulate pivots, locate pixels by color, covert pixel positions to UV offsets, an remove specific pixel colors. Useful for dynamically adjusting sprite origins, detecting color positions, or creating modified textures at runtime.
    /// </summary>
    public static class SpriteExtensions
    {
        private static Color32[] GetSpritePixels(Sprite sprite, out int width, out int height)
        {
            Rect rect = sprite.textureRect;
            int x = Mathf.RoundToInt(rect.x);
            int y = Mathf.RoundToInt(rect.y);
            width = Mathf.RoundToInt(rect.width);
            height = Mathf.RoundToInt(rect.height);

            Color32[] texturePixels = sprite.texture.GetPixels32();
            var spritePixels = new Color32[width * height];
            int textureWidth = sprite.texture.width;
            for (int row = 0; row < height; row++)
            {
                Array.Copy(texturePixels, (y + row) * textureWidth + x, spritePixels, row * width, width);
            }

            return spritePixels;
        }

        private static Vector2 GetSpritePivot(Sprite sprite, int width, int height, int padding = 0)
        {
            return new Vector2(
                (sprite.pivot.x - sprite.textureRectOffset.x + padding) / width,
                (sprite.pivot.y - sprite.textureRectOffset.y + padding) / height
            );
        }

        private static Sprite CreateSprite(Sprite source, Color32[] pixels, int width, int height, int pivotPadding = 0)
        {
            var texture = new Texture2D(width, height)
            {
                filterMode = source.texture.filterMode,
                wrapMode = source.texture.wrapMode
            };
            texture.SetPixels32(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, width, height),
                GetSpritePivot(source, width, height, pivotPadding),
                source.pixelsPerUnit
            );
        }

        private static byte GetAlphaThreshold(float alphaThreshold)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(alphaThreshold * 255f), 0, 255);
        }

        private static bool HasVisiblePixels(Color32[] pixels, int textureWidth, int x, int y, int width, int height)
        {
            int yEnd = y + height;
            int xEnd = x + width;
            for (int row = y; row < yEnd; row++)
            {
                int rowOffset = row * textureWidth;
                for (int column = x; column < xEnd; column++)
                {
                    if (pixels[rowOffset + column].a > 2)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Vector2 representing the center of a sprite.
        /// </summary>
        public static readonly Vector2 PivotCenter = new(0.5f, 0.5f);

        /// <summary>
        /// Centers the pivot of a sprite to (0.5, 0.5).
        /// </summary>
        /// <param name="sprite">Original sprite.</param>
        /// <returns>Sprite with a pivot of 0.5, 0.5</returns>
        public static Sprite CenterSprite(this Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return null;

            return Sprite.Create(sprite.texture, sprite.textureRect, PivotCenter, sprite.pixelsPerUnit);
        }

        /// <summary>
        /// Find the (first) pixel coordinate for a given color.
        /// </summary>
        /// <param name="sprite">Sprite to search.</param>
        /// <param name="color">Color to look for.</param>
        /// <returns>Pixel coordinate of the first found color.</returns>
        public static Vector2Int GetPixelPoint(this Sprite sprite, Color32 color)
        {
            if (sprite != null && sprite.TryGetPixelPoint(color, out Vector2Int point))
                return point;

            Debug.LogError("Could not find the pixel point!");
            return Vector2Int.zero;
        }

        /// <summary>
        /// Finds the first pixel coordinate matching a color, relative to the sprite's texture rectangle.
        /// </summary>
        public static bool TryGetPixelPoint(this Sprite sprite, Color32 color, out Vector2Int point)
        {
            point = Vector2Int.zero;
            if (sprite == null || sprite.texture == null || !sprite.texture.isReadable)
                return false;

            Color32[] pixels = GetSpritePixels(sprite, out int width, out int height);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (!pixels[i].Equals(color))
                    continue;

                point = new Vector2Int(i % width, i / width);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Given a pixel point on a sprite, find the normalized offset.
        /// </summary>
        /// <param name="sprite">Sprite to grab the texture rect from.</param>
        /// <param name="pixelPoint">Pixel point to use as reference.</param>
        /// <returns>Normalized offset.</returns>
        public static Vector2 PixelPointToOffset(this Sprite sprite, Vector2Int pixelPoint)
        {
            if (sprite == null)
                return Vector2.zero;

            var rect = sprite.textureRect;
            return new Vector2(pixelPoint.x / rect.width, pixelPoint.y / rect.height);
        }

        /// <summary>
        /// Remove a list of colours from a sprite.
        /// </summary>
        /// <param name="sprite">Sprite to search and filter.</param>
        /// <param name="colors">Colors to find and remove.</param>
        /// <returns>Sprite with removed colours.</returns>
        public static Sprite RemovePixelPoints(this Sprite sprite, params Color32[] colors)
        {
            if (sprite == null || sprite.texture == null)
                return null;

            Color32[] pixels = GetSpritePixels(sprite, out int width, out int height);
            var colorsToRemove = colors == null ? new HashSet<Color32>() : new HashSet<Color32>(colors);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (colorsToRemove.Contains(pixels[i]))
                    pixels[i] = new Color32(0, 0, 0, 0);
            }

            return CreateSprite(sprite, pixels, width, height);
        }

        /// <summary>
        /// Convert the sprite so that all pixels with alpha greater than <c>alphaThreshold</c> become the provided <c>color</c>,
        /// and all other pixels become clear (transparent).
        /// </summary>
        /// <param name="sprite">Source sprite whose texture rect will be processed.</param>
        /// <param name="color">Target color to apply to pixels above the threshold.</param>
        /// <param name="alphaThreshold">Alpha threshold in range [0,1]. Pixels with alpha &gt; threshold become <paramref name="color"/>. Default is 0.5.</param>
        /// <returns>A new <see cref="Sprite"/> with converted pixels.</returns>
        public static Sprite ToColor(this Sprite sprite, Color32 color, float alphaThreshold = 0.5f)
        {
            if (sprite == null || sprite.texture == null)
                return null;

            Color32[] pixels = GetSpritePixels(sprite, out int width, out int height);

            // Convert pixels based on alpha threshold
            byte thresh = GetAlphaThreshold(alphaThreshold);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > thresh)
                    pixels[i] = color;
                else
                    pixels[i] = Color.clear;
            }

            return CreateSprite(sprite, pixels, width, height);
        }

        /// <summary>
        /// Adds a pixel outline to a sprite, expanding the texture bounds to ensure outlines appear even at edges.
        /// </summary>
        /// <param name="sprite">The source sprite to add an outline to.</param>
        /// <param name="outlineColor">The color to use for the outline.</param>
        /// <param name="cutCorners">If true, uses 4-neighbour outline (cross pattern). If false, uses 8-neighbour (including diagonals).</param>
        /// <param name="alphaThreshold">Alpha value below which pixels are treated as transparent. Allows semi-transparent pixels to be ignored.</param>
        /// <returns>A new sprite with the outline applied.</returns>
        public static Sprite AddOutline(this Sprite sprite, Color32 outlineColor, bool cutCorners = false, byte alphaThreshold = 0)
        {
            if (sprite == null || sprite.texture == null)
                return null;

            Color32[] pixels = GetSpritePixels(sprite, out int width, out int height);

            // Create expanded buffer with 1 pixel padding on all sides
            int expandedWidth = width + 2;
            int expandedHeight = height + 2;
            var expandedPixels = new Color32[expandedWidth * expandedHeight];
            
            // Copy original pixels into center of expanded buffer
            for (int y = 0; y < height; y++)
            {
                Array.Copy(pixels, y * width, expandedPixels, (y + 1) * expandedWidth + 1, width);
            }

            // Output buffer starts as a copy of the expanded pixels
            var outPixels = new Color32[expandedWidth * expandedHeight];
            Array.Copy(expandedPixels, outPixels, expandedPixels.Length);

            // helper to check if a pixel is opaque (alpha > threshold)
            bool IsOpaque(in Color32 c) => c.a > alphaThreshold;

            // Check transparent pixels only; if any neighbour (4 or 8 depending on cutCorners) is opaque set outline color
            for (int y = 0; y < expandedHeight; y++)
            {
                int row = y * expandedWidth;
                for (int x = 0; x < expandedWidth; x++)
                {
                    int idx = row + x;
                    if (IsOpaque(expandedPixels[idx])) continue; // preserve existing opaque

                    bool neighbourOpaque = false;

                    if (cutCorners)
                    {
                        // 4-neighbour check (up, down, left, right)
                        if (x > 0 && IsOpaque(expandedPixels[idx - 1])) neighbourOpaque = true;
                        else if (x < expandedWidth - 1 && IsOpaque(expandedPixels[idx + 1])) neighbourOpaque = true;
                        else if (y > 0 && IsOpaque(expandedPixels[idx - expandedWidth])) neighbourOpaque = true;
                        else if (y < expandedHeight - 1 && IsOpaque(expandedPixels[idx + expandedWidth])) neighbourOpaque = true;
                    }
                    else
                    {
                        // 8-neighbour check (including diagonals)
                        int y0 = Math.Max(0, y - 1);
                        int y1 = Math.Min(expandedHeight - 1, y + 1);
                        int x0 = Math.Max(0, x - 1);
                        int x1 = Math.Min(expandedWidth - 1, x + 1);

                        for (int ny = y0; ny <= y1 && !neighbourOpaque; ny++)
                        {
                            int nRow = ny * expandedWidth;
                            for (int nx = x0; nx <= x1; nx++)
                            {
                                if (nx == x && ny == y) continue;
                                if (IsOpaque(expandedPixels[nRow + nx]))
                                {
                                    neighbourOpaque = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (neighbourOpaque)
                    {
                        outPixels[idx] = outlineColor;
                    }
                }
            }

            return CreateSprite(sprite, outPixels, expandedWidth, expandedHeight, 1);
        }

        /// <summary>
        /// Divides a sprite sheet into a 2D list of sprites (rows x columns).
        /// Only non-empty cells (pixels with alpha &gt; 0.01) are included.
        /// Notes:
        /// - The texture used by the sprite must be Read/Write enabled in import settings for pixel checks to work.
        /// - If the texture is not readable, the function will still create sprites but will not be able to detect emptiness and will include the cell.
        /// </summary>
        /// <param name="sheet">Source sprite (texture) containing the sheet.</param>
        /// <param name="rows">Number of rows to divide into (vertical slices).</param>
        /// <param name="columns">Number of columns to divide into (horizontal slices).</param>
        /// <param name="pixelsPerUnit">Pixels per unit for created sprites (defaults to 100).</param>
        /// <param name="pivot">Optional pivot point for created sprites (defaults to Unity’s (0.5, 0.5)).</param>
        /// <returns>List of rows, each row is a list of Sprites (empty rows are omitted).</returns>
        public static List<List<Sprite>> DivideSpriteSheet(this Sprite sheet, int rows, int columns, int pixelsPerUnit = 100, Vector2? pivot = null)
        {
            var result = new List<List<Sprite>>();
            if (sheet == null || rows <= 0 || columns <= 0)
                return result;

            Texture2D tex = sheet.texture;
            if (tex == null)
                return result;

            int texW = tex.width;
            int texH = tex.height;

            int cellW = texW / columns;
            int cellH = texH / rows;

            if (cellW <= 0 || cellH <= 0)
                return result;

            Vector2 actualPivot = pivot ?? new Vector2(0.5f, 0.5f);

            bool readable = tex.isReadable;
            Color32[] allPixels = readable ? tex.GetPixels32() : null;

            for (int r = 0; r < rows; r++)
            {
                var rowList = new List<Sprite>(columns);
                for (int c = 0; c < columns; c++)
                {
                    int x = c * cellW;
                    int y = r * cellH;

                    int w = (c == columns - 1) ? texW - x : cellW;
                    int h = (r == rows - 1) ? texH - y : cellH;

                    if (w <= 0 || h <= 0)
                        continue;

                    if (readable && !HasVisiblePixels(allPixels, texW, x, y, w, h))
                        continue;

                    Rect rect = new Rect(x, y, w, h);
                    Sprite s = Sprite.Create(tex, rect, actualPivot, pixelsPerUnit);
                    rowList.Add(s);
                }

                if (rowList.Count > 0)
                    result.Add(rowList);
            }

            return result;
        }

        /// <summary>
        /// Divides a sprite sheet into a flat, row-major list, skipping fully transparent cells.
        /// </summary>
        public static List<Sprite> DivideSpriteSheetFlat(this Sprite sheet, int rows, int columns, int pixelsPerUnit = 100, Vector2? pivot = null)
        {
            List<List<Sprite>> rowSprites = sheet.DivideSpriteSheet(rows, columns, pixelsPerUnit, pivot);
            var result = new List<Sprite>();
            foreach (List<Sprite> row in rowSprites)
                result.AddRange(row);
            return result;
        }

        /// <summary>
        /// Calculates the pixel width of a sprite within its texture rect, counting only pixels with alpha above the threshold.
        /// </summary>
        /// <param name="sprite">The sprite to measure.</param>
        /// <param name="alphaThreshold">The alpha threshold (0-1). Pixels with alpha above this value are counted.</param>
        /// <returns>The width in pixels of the sprite's visible content within its texture rect.</returns>
        public static int GetPixelWidth(this Sprite sprite, float alphaThreshold = 0.5f)
        {
            if (sprite == null || sprite.texture == null)
                return 0;

            Color32[] pixels = GetSpritePixels(sprite, out int width, out int height);
            byte threshold = GetAlphaThreshold(alphaThreshold);
            int minX = width;
            int maxX = -1;

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a <= threshold)
                    continue;

                int x = i % width;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
            }

            return maxX < 0 ? 0 : maxX - minX + 1;
        }
    }
}
