using System.Globalization;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.Tesseract {
    public static class TesseractUtils {
        public static void Load() {
            On.Celeste.Tags.Initialize += AddTesseractTag;
        }

        public static void Unload() {
            On.Celeste.Tags.Initialize -= AddTesseractTag;
        }

        private static void AddTesseractTag(On.Celeste.Tags.orig_Initialize orig) {
            orig();
            TesseractTags.Tesseract = new BitTag("Tesseract");
        }

        public static Vector3 Vec3(this EntityData data, string key, Vector3 defaultValue = default(Vector3)) {
            if (data.Values.TryGetValue(key, out object result)) {
                if (result is Vector3 vec) {
                    return vec;
                }

                var parts = result.ToString().Split(',');
                if (parts.Length != 3) { return defaultValue; }
                if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) { return defaultValue; }
                if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) { return defaultValue; }
                if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) { return defaultValue; }

                return new Vector3(x, y, z);
            }
            return defaultValue;
        }
    }

    public static class TesseractTags {
        public static BitTag Tesseract;
    }
}
