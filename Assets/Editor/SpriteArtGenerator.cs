using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SweetBreaker.EditorTools
{
    /// <summary>
    /// Draws every sprite in Assets/Art/Sprites from simple shapes, so all of the game's art is our
    /// own work with no licence to track (GDD section 6). Run it from Sweet Breaker > Regenerate Sprites.
    /// Shapes are signed-distance functions: negative inside, positive outside, in pixels.
    /// </summary>
    public static class SpriteArtGenerator
    {
        private const string SpriteFolder = "Assets/Art/Sprites";

        // Transparent margin around bricks, ball and capsule that holds their drop shadow.
        private const int ShadowPad = 8;

        private static readonly Color Shadow = new Color(0.24f, 0.12f, 0.06f, 0.32f);
        private static readonly Color Cream = Hex("#FFF4E4");

        [MenuItem("Sweet Breaker/Regenerate Sprites")]
        public static void RegenerateAll()
        {
            Directory.CreateDirectory(SpriteFolder);

            DrawCandyBrick("CandyBrick_Pink", Hex("#FF7FAE"));
            DrawCandyBrick("CandyBrick_Mint", Hex("#5FD6B0"));
            DrawCandyBrick("CandyBrick_Lemon", Hex("#FFD23F"));
            DrawChocolateBrick("ChocolateBrick", cracked: false);
            DrawChocolateBrick("ChocolateBrick_Cracked", cracked: true);
            DrawPaddle();
            DrawBall();
            DrawCapsule();
            DrawWallTile();
            DrawBackground();

            AssetDatabase.Refresh();
            ApplyImportSettings();
            Debug.Log("Sweet Breaker sprites regenerated.");
        }

        // ---------------------------------------------------------------- bricks

        /// <summary>A wrapped candy: a glossy striped body with crimped wrapper ends (1.4 x 0.5 u).</summary>
        private static void DrawCandyBrick(string name, Color candy)
        {
            var canvas = new Canvas(140 + 2 * ShadowPad, 50 + 2 * ShadowPad);
            Vector2 c = canvas.Centre;
            Color light = Color.Lerp(candy, Color.white, 0.35f);
            Color dark = Color.Lerp(candy, Color.black, 0.3f);
            Color rim = Color.Lerp(candy, Color.black, 0.5f);
            Color wrapper = Color.Lerp(candy, Color.white, 0.55f);

            Func<Vector2, float> body = p => Sdf.RoundedRect(p, c, new Vector2(49f, 22f), 20f);
            Func<Vector2, float> leftEnd = p => WrapperEnd(p, c, -1f);
            Func<Vector2, float> rightEnd = p => WrapperEnd(p, c, 1f);
            Func<Vector2, float> whole = p => Mathf.Min(body(p), Mathf.Min(leftEnd(p), rightEnd(p)));

            canvas.FillSoft(p => whole(p + new Vector2(0f, 4f)), Shadow, 3f);

            // Wrapper ends first, so the candy body sits on top of them.
            foreach (Func<Vector2, float> end in new[] { leftEnd, rightEnd })
            {
                canvas.Fill(end, p => Color.Lerp(Color.Lerp(wrapper, candy, 0.35f), wrapper, (p.y - c.y + 24f) / 48f));
                for (int i = -1; i <= 1; i++)
                {
                    float side = end == leftEnd ? -1f : 1f;
                    var pinch = new Vector2(c.x + side * 46f, c.y + i * 3f);
                    var fan = new Vector2(c.x + side * 68f, c.y + i * 17f);
                    canvas.Fill(p => Mathf.Max(end(p) + 1.5f, Sdf.Segment(p, pinch, fan, 0.7f)), WithAlpha(rim, 0.45f));
                }
                canvas.Fill(p => Mathf.Abs(end(p) + 0.8f) - 0.8f, rim);
            }

            canvas.Fill(body, p => Color.Lerp(dark, light, (p.y - c.y + 22f) / 44f));
            canvas.Fill(p => Mathf.Max(body(p) + 2f, Mathf.Abs(Mathf.Repeat(p.x + p.y * 0.9f, 20f) - 10f) - 3.5f), WithAlpha(Color.white, 0.35f));
            canvas.Fill(p => Sdf.RoundedRect(p, c + new Vector2(0f, 11f), new Vector2(34f, 4.5f), 4.5f), WithAlpha(Color.white, 0.55f));
            canvas.Fill(p => Sdf.Circle(p, c + new Vector2(-30f, 10f), 3f), WithAlpha(Color.white, 0.9f));
            canvas.Fill(p => Mathf.Abs(body(p) + 1f) - 1f, rim);

            canvas.Save(name);
        }

        /// <summary>One twisted wrapper end: pinched at the candy, fanning out to a crimped edge.</summary>
        private static float WrapperEnd(Vector2 p, Vector2 c, float side)
        {
            float innerX = c.x + side * 44f;
            float outerX = c.x + side * 70f;
            float t = Mathf.Clamp01((p.x - innerX) / (outerX - innerX));
            float halfHeight = Mathf.Lerp(8f, 24f, t * t * (3f - 2f * t));

            // The crimped outer edge zigzags in and out by 2 px.
            float crimp = 2f * Mathf.Abs(Mathf.Repeat(p.y, 6f) - 3f) / 3f;
            float alongX = side * (p.x - outerX) + crimp;
            float acrossY = Mathf.Abs(p.y - c.y) - halfHeight;
            float behindInner = side * (innerX - p.x);
            return Mathf.Max(Mathf.Max(alongX, acrossY), behindInner);
        }

        /// <summary>
        /// A chocolate bar scored into 4 x 2 squares. The cracked state is lighter, split by a deep
        /// crack that shows the lighter inside, and missing a bitten-off corner, so it reads mid-rally.
        /// </summary>
        private static void DrawChocolateBrick(string name, bool cracked)
        {
            var canvas = new Canvas(140 + 2 * ShadowPad, 50 + 2 * ShadowPad);
            Vector2 c = canvas.Centre;
            Color bodyColour = Hex(cracked ? "#8E5A3A" : "#5A2F1C");
            Color squareColour = Hex(cracked ? "#A56C47" : "#6E3B24");
            Color rim = Hex("#2B150A");
            Color inside = Hex("#D7A574");

            Func<Vector2, float> bar = p => Sdf.RoundedRect(p, c, new Vector2(70f, 25f), 7f);
            Func<Vector2, float> bite = p => Sdf.Polygon(p, new[]
            {
                c + new Vector2(34f, 27f), c + new Vector2(40f, 14f), c + new Vector2(52f, 9f),
                c + new Vector2(60f, -1f), c + new Vector2(72f, -3f), c + new Vector2(72f, 27f),
            });
            Func<Vector2, float> body = cracked ? p => Mathf.Max(bar(p), -bite(p)) : bar;

            canvas.FillSoft(p => body(p + new Vector2(0f, 4f)), Shadow, 3f);
            canvas.Fill(body, p => Color.Lerp(Color.Lerp(bodyColour, Color.black, 0.2f), bodyColour, (p.y - c.y + 25f) / 50f));

            for (int column = 0; column < 4; column++)
            {
                for (int row = 0; row < 2; row++)
                {
                    Vector2 centre = c + new Vector2(-51f + column * 34f, -11.5f + row * 23f);
                    Func<Vector2, float> square = p => Mathf.Max(Sdf.RoundedRect(p, centre, new Vector2(13.5f, 8.5f), 3f), body(p) + 2f);
                    canvas.Fill(p => square(p + new Vector2(-1.5f, 1.5f)), Color.Lerp(squareColour, Color.black, 0.35f));
                    canvas.Fill(p => square(p + new Vector2(1.2f, -1.2f)), Color.Lerp(squareColour, Color.white, 0.25f));
                    canvas.Fill(square, p => Color.Lerp(squareColour, Color.Lerp(squareColour, Color.white, 0.12f), (p.y - centre.y + 8.5f) / 17f));
                }
            }

            canvas.Fill(p => Mathf.Max(Sdf.RoundedRect(p, c + new Vector2(0f, 17f), new Vector2(62f, 3f), 3f), body(p) + 1f), WithAlpha(Color.white, cracked ? 0.12f : 0.18f));

            if (cracked)
            {
                var crack = new[]
                {
                    c + new Vector2(-6f, 27f), c + new Vector2(2f, 15f), c + new Vector2(-7f, 5f),
                    c + new Vector2(6f, -6f), c + new Vector2(-2f, -15f), c + new Vector2(5f, -27f),
                };
                var branch = new[] { c + new Vector2(-7f, 5f), c + new Vector2(-24f, 0f), c + new Vector2(-33f, 8f) };
                canvas.Fill(p => Mathf.Max(Sdf.Polyline(p, crack, 3.4f), body(p)), rim);
                canvas.Fill(p => Mathf.Max(Sdf.Polyline(p, crack, 1.4f), body(p) + 1f), inside);
                canvas.Fill(p => Mathf.Max(Sdf.Polyline(p, branch, 2.2f), body(p)), rim);
                canvas.Fill(p => Mathf.Max(Sdf.Polyline(p, branch, 0.8f), body(p) + 1f), inside);

                // The broken edge where the corner was bitten off shows the lighter inside too.
                canvas.Fill(p => Mathf.Max(Mathf.Abs(bite(p) - 1.5f) - 1.5f, bar(p) + 0.5f), inside);
            }

            canvas.Fill(p => Mathf.Abs(body(p) + 1f) - 1f, rim);
            canvas.Save(name);
        }

        // ---------------------------------------------------------------- paddle, ball, capsule

        /// <summary>
        /// A wrapped chocolate bar, 2.2 x 0.4 u. Only the ends vary along X: the middle is uniform, so
        /// 9-slicing stretches the wrapper for the power-up and never the rounded foil ends.
        /// The padding is vertical only, so the sprite's width is still the paddle's width.
        /// </summary>
        private static void DrawPaddle()
        {
            var canvas = new Canvas(220, 40 + 2 * ShadowPad);
            Vector2 c = canvas.Centre;
            Func<Vector2, float> body = p => Sdf.RoundedRect(p, c, new Vector2(109f, 19f), 17f);
            Func<Vector2, float> foil = p => Mathf.Max(body(p), Mathf.Min(p.x - 22f, 198f - p.x) >= 0f ? 1f : -1f);

            canvas.FillSoft(p => Sdf.RoundedRect(p, c + new Vector2(0f, -5f), new Vector2(104f, 17f), 16f), Shadow, 3f);
            canvas.Fill(body, p => Color.Lerp(Hex("#8E1630"), Hex("#E43E5C"), (p.y - c.y + 19f) / 38f));
            canvas.Fill(p => Mathf.Max(body(p), Mathf.Abs(p.y - c.y + 1f) - 5f), p => Color.Lerp(Hex("#C98F1E"), Hex("#FFD66B"), (p.y - c.y + 6f) / 10f));
            canvas.Fill(p => Mathf.Max(body(p), Mathf.Abs(p.y - c.y + 1f) - 0.6f), WithAlpha(Hex("#8A5A00"), 0.6f));
            canvas.Fill(p => Sdf.RoundedRect(p, c + new Vector2(0f, 11f), new Vector2(96f, 3f), 3f), WithAlpha(Color.white, 0.45f));

            canvas.Fill(foil, p => Color.Lerp(Hex("#9EA2AE"), Hex("#F2F3F8"), (p.y - c.y + 19f) / 38f));
            foreach (float x in new[] { 6f, 11f, 16f, 204f, 209f, 214f })
                canvas.Fill(p => Mathf.Max(foil(p), Mathf.Abs(p.x - x) - 0.7f), WithAlpha(Hex("#6D7080"), 0.7f));
            canvas.Fill(p => Mathf.Max(body(p) + 1f, Mathf.Min(Mathf.Abs(p.x - 22f), Mathf.Abs(p.x - 198f)) - 1f), Hex("#5A0E1E"));
            canvas.Fill(p => Mathf.Abs(body(p) + 1f) - 1f, Hex("#4A0A18"));

            canvas.Save("Paddle");
        }

        /// <summary>A white gumball, 0.32 u, with a soft shadow.</summary>
        private static void DrawBall()
        {
            var canvas = new Canvas(32 + 2 * ShadowPad, 32 + 2 * ShadowPad);
            Vector2 c = canvas.Centre;
            canvas.FillSoft(p => Sdf.Circle(p, c + new Vector2(2f, -3f), 14.5f), Shadow, 2.5f);
            canvas.Fill(p => Sdf.Circle(p, c, 15.2f), p => Color.Lerp(Color.white, Hex("#C4CAD8"), Mathf.Clamp01((p - (c + new Vector2(-5f, 5f))).magnitude / 22f)));
            canvas.Fill(p => Sdf.Circle(p, c + new Vector2(-5f, 5.5f), 4f), WithAlpha(Color.white, 0.95f));
            canvas.Fill(p => Mathf.Abs(Sdf.Circle(p, c, 14.6f)) - 0.6f, Hex("#A7AEBE"));
            canvas.Save("Ball");
        }

        /// <summary>The paddle-expansion capsule: half pink, half white, with an outward double arrow.</summary>
        private static void DrawCapsule()
        {
            var canvas = new Canvas(80 + 2 * ShadowPad, 36 + 2 * ShadowPad);
            Vector2 c = canvas.Centre;
            Func<Vector2, float> pill = p => Sdf.RoundedRect(p, c, new Vector2(39f, 17f), 16.5f);
            canvas.FillSoft(p => pill(p + new Vector2(-2f, 4f)), Shadow, 3f);
            canvas.Fill(pill, p => p.x < c.x
                ? Color.Lerp(Hex("#E0457F"), Hex("#FF8DB8"), (p.y - c.y + 17f) / 34f)
                : Color.Lerp(Hex("#DADDE6"), Color.white, (p.y - c.y + 17f) / 34f));
            canvas.Fill(p => Sdf.RoundedRect(p, c + new Vector2(0f, 9f), new Vector2(30f, 3f), 3f), WithAlpha(Color.white, 0.5f));
            Color arrow = Hex("#5A2340");
            canvas.Fill(p => Mathf.Min(
                Sdf.Segment(p, c + new Vector2(-14f, 0f), c + new Vector2(14f, 0f), 1.8f),
                Mathf.Min(
                    Mathf.Min(Sdf.Segment(p, c + new Vector2(-14f, 0f), c + new Vector2(-8f, 6f), 1.8f), Sdf.Segment(p, c + new Vector2(-14f, 0f), c + new Vector2(-8f, -6f), 1.8f)),
                    Mathf.Min(Sdf.Segment(p, c + new Vector2(14f, 0f), c + new Vector2(8f, 6f), 1.8f), Sdf.Segment(p, c + new Vector2(14f, 0f), c + new Vector2(8f, -6f), 1.8f)))), arrow);
            canvas.Fill(p => Mathf.Abs(pill(p) + 1f) - 1f, Hex("#9E2F5E"));
            canvas.Save("PowerUpCapsule");
        }

        // ---------------------------------------------------------------- walls and background

        /// <summary>A wafer tile with grooved cross-hatching; it tiles seamlessly every 50 px.</summary>
        private static void DrawWallTile()
        {
            var canvas = new Canvas(50, 50);
            canvas.Fill(p => -1f, p => Color.Lerp(Hex("#D9A15E"), Hex("#EDC287"), 0.5f + 0.5f * Mathf.Sin(p.x * Mathf.PI / 25f)));
            Func<Vector2, float> grooves = p => Mathf.Min(
                Mathf.Abs(Mathf.Repeat(p.x + p.y, 25f) - 12.5f),
                Mathf.Abs(Mathf.Repeat(p.x - p.y, 25f) - 12.5f));
            canvas.Fill(p => grooves(p + new Vector2(0.8f, -0.8f)) - 1.6f, WithAlpha(Hex("#FFE0AE"), 0.8f));
            canvas.Fill(p => grooves(p) - 1.6f, Hex("#B7793B"));
            canvas.Save("WallTile");
        }

        /// <summary>
        /// The confectionery counter, 26 x 14 u centred on the play field: a pink gingham cloth, with a
        /// lighter tray under the play area scattered with faint sprinkles.
        /// </summary>
        private static void DrawBackground()
        {
            var canvas = new Canvas(2600, 1400);
            Color pinkCheck = new Color(1f, 0.55f, 0.7f, 0.16f);

            canvas.Fill(p => -1f, Cream);
            canvas.Fill(p => Mathf.Abs(Mathf.Repeat(p.x, 80f) - 40f) - 20f, pinkCheck);
            canvas.Fill(p => Mathf.Abs(Mathf.Repeat(p.y, 80f) - 40f) - 20f, pinkCheck);

            // The tray covers the field inside the walls (x within +-7.5 u) and runs off the bottom edge.
            var tray = new RectInt(550, 0, 1500, 1150);
            canvas.Fill(p => Sdf.RoundedRect(p, new Vector2(1300f, 500f), new Vector2(750f, 650f), 24f),
                p =>
                {
                    float edge = Mathf.Min(Mathf.Min(p.x - tray.xMin, tray.xMax - p.x), tray.yMax - p.y);
                    return Color.Lerp(Hex("#F3DCC2"), Hex("#FFFAF2"), Mathf.Clamp01(edge / 40f));
                }, new RectInt(tray.xMin - 2, tray.yMin, tray.width + 4, tray.height + 2));

            var random = new System.Random(2026);
            Color[] sprinkleColours = { Hex("#FF7FAE"), Hex("#5FD6B0"), Hex("#FFD23F"), Hex("#8FB8FF"), Hex("#B98CFF") };
            for (int i = 0; i < 300; i++)
            {
                var centre = new Vector2(tray.xMin + 30 + random.Next(tray.width - 60), tray.yMin + 20 + random.Next(tray.height - 60));
                float angle = (float)random.NextDouble() * Mathf.PI;
                var half = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 6f;
                // Faint, so nothing on the tray competes with the ball for attention (GDD pillar 1).
                Color colour = WithAlpha(sprinkleColours[random.Next(sprinkleColours.Length)], 0.2f);
                canvas.Fill(p => Sdf.Segment(p, centre - half, centre + half, 2.4f), colour,
                    new RectInt((int)centre.x - 10, (int)centre.y - 10, 20, 20));
            }

            canvas.Save("Background");
        }

        // ---------------------------------------------------------------- import settings

        private static void ApplyImportSettings()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SpriteFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = Path.GetFileNameWithoutExtension(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                bool isBackground = file == "Background";
                bool needsFullRect = file == "Paddle" || file == "WallTile" || file == "UIRounded";

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = file == "WallTile" ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.textureCompression = isBackground ? TextureImporterCompression.Compressed : TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = isBackground ? 4096 : 2048;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = needsFullRect ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
                importer.SetTextureSettings(settings);

                // 9-slice borders: the paddle's foil ends, and the UI panel's rounded corners.
                if (file == "Paddle")
                    importer.spriteBorder = new Vector4(26, 0, 26, 0);
                if (file == "UIRounded")
                    importer.spriteBorder = new Vector4(24, 24, 24, 24);

                importer.SaveAndReimport();
            }
        }

        // ---------------------------------------------------------------- helpers

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color colour);
            return colour;
        }

        private static Color WithAlpha(Color colour, float alpha)
        {
            colour.a = alpha;
            return colour;
        }

        /// <summary>An RGBA pixel buffer that shapes are painted onto with anti-aliased edges.</summary>
        private class Canvas
        {
            private readonly int width;
            private readonly int height;
            private readonly Color[] pixels;

            public Canvas(int width, int height)
            {
                this.width = width;
                this.height = height;
                pixels = new Color[width * height];
            }

            public Vector2 Centre => new Vector2(width * 0.5f, height * 0.5f);

            public void Fill(Func<Vector2, float> shape, Color colour) => Fill(shape, p => colour);

            public void Fill(Func<Vector2, float> shape, Color colour, RectInt bounds) => Fill(shape, p => colour, bounds);

            public void Fill(Func<Vector2, float> shape, Func<Vector2, Color> paint) =>
                Fill(shape, paint, new RectInt(0, 0, width, height));

            /// <summary>Paints where the shape is, with a one-pixel anti-aliased edge.</summary>
            public void Fill(Func<Vector2, float> shape, Func<Vector2, Color> paint, RectInt bounds)
            {
                ForEachPixel(bounds, p => Mathf.Clamp01(0.5f - shape(p)), paint);
            }

            /// <summary>Paints a blurred copy of the shape, for drop shadows.</summary>
            public void FillSoft(Func<Vector2, float> shape, Color colour, float softness)
            {
                ForEachPixel(new RectInt(0, 0, width, height),
                    p => 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-softness, softness, shape(p))),
                    p => colour);
            }

            public void Save(string name)
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(Path.Combine(SpriteFolder, name + ".png"), texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }

            private void ForEachPixel(RectInt bounds, Func<Vector2, float> coverage, Func<Vector2, Color> paint)
            {
                int xMin = Mathf.Max(0, bounds.xMin), xMax = Mathf.Min(width, bounds.xMax);
                int yMin = Mathf.Max(0, bounds.yMin), yMax = Mathf.Min(height, bounds.yMax);
                for (int y = yMin; y < yMax; y++)
                {
                    for (int x = xMin; x < xMax; x++)
                    {
                        var p = new Vector2(x + 0.5f, y + 0.5f);
                        float amount = coverage(p);
                        if (amount > 0f)
                            Blend(y * width + x, paint(p), amount);
                    }
                }
            }

            // Standard "over" compositing of a straight-alpha colour onto the pixel.
            private void Blend(int index, Color colour, float amount)
            {
                float alpha = colour.a * amount;
                if (alpha <= 0f)
                    return;

                Color below = pixels[index];
                float outAlpha = alpha + below.a * (1f - alpha);
                Color result = (colour * alpha + below * below.a * (1f - alpha)) / outAlpha;
                result.a = outAlpha;
                pixels[index] = result;
            }
        }

        /// <summary>Signed distance functions for the shapes the sprites are built from.</summary>
        private static class Sdf
        {
            public static float Circle(Vector2 p, Vector2 centre, float radius) => (p - centre).magnitude - radius;

            public static float RoundedRect(Vector2 p, Vector2 centre, Vector2 halfSize, float radius)
            {
                Vector2 q = new Vector2(Mathf.Abs(p.x - centre.x), Mathf.Abs(p.y - centre.y)) - halfSize + Vector2.one * radius;
                Vector2 outside = Vector2.Max(q, Vector2.zero);
                return outside.magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
            }

            public static float Segment(Vector2 p, Vector2 a, Vector2 b, float halfWidth)
            {
                Vector2 ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                return (p - (a + ab * t)).magnitude - halfWidth;
            }

            public static float Polyline(Vector2 p, Vector2[] points, float halfWidth)
            {
                float distance = float.MaxValue;
                for (int i = 0; i < points.Length - 1; i++)
                    distance = Mathf.Min(distance, Segment(p, points[i], points[i + 1], halfWidth));
                return distance;
            }

            /// <summary>Any simple polygon: distance to the nearest edge, negative when inside.</summary>
            public static float Polygon(Vector2 p, Vector2[] points)
            {
                float distance = float.MaxValue;
                bool inside = false;
                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                {
                    Vector2 a = points[j], b = points[i];
                    distance = Mathf.Min(distance, Segment(p, a, b, 0f));
                    if ((b.y > p.y) != (a.y > p.y) && p.x < (a.x - b.x) * (p.y - b.y) / (a.y - b.y) + b.x)
                        inside = !inside;
                }
                return inside ? -distance : distance;
            }
        }
    }
}
