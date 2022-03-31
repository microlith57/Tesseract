using System;

namespace Celeste.Mod.Tesseract {
    public class TesseractModule : EverestModule {
        public static TesseractModule Instance;

        public TesseractModule() {
            Instance = this;
        }

        public override void Load() {
            TesseractUtils.Load();
            TesseractGameplayRenderer.Load();
        }

        public override void Unload() {
            TesseractUtils.Unload();
            TesseractGameplayRenderer.Unload();
        }

        public override void LoadContent(bool firstLoad) {
            Teapot.LoadData();
        }
    }
}
