namespace Celeste.Mod.Tesseract {
    public class TesseractModule : EverestModule {
        public static TesseractModule Instance;

        public TesseractModule() {
            Instance = this;
        }

        public override void Load() {
            TesseractUtils.Load();
            TesseractGameplayRenderer.Load();

            On.Celeste.GameplayBuffers.Create += OnCreateBuffers;
            On.Celeste.GameplayBuffers.Unload += OnUnloadBuffers;
        }

        public override void Unload() {
            TesseractUtils.Unload();
            TesseractGameplayRenderer.Unload();

            On.Celeste.GameplayBuffers.Create -= OnCreateBuffers;
            On.Celeste.GameplayBuffers.Unload -= OnUnloadBuffers;
        }

        public override void LoadContent(bool firstLoad) {
            Teapot.LoadData();
            BillboardRenderer.LoadData();
        }

        public static void OnCreateBuffers(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();
            TesseractLevel.CreateBuffers();
            BillboardRenderer.CreateBuffers();
        }

        public static void OnUnloadBuffers(On.Celeste.GameplayBuffers.orig_Unload orig) {
            orig();
            TesseractLevel.UnloadBuffers();
            BillboardRenderer.UnloadBuffers();
        }
    }
}
