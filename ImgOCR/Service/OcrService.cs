using Tesseract;

namespace ImgOCR.Service
{
    public class OcrService
    {
        public string ExtractTextFromImage(byte[] imageBytes)
        {
            try
            {
                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(imageBytes))
                    {
                        using (var page = engine.Process(img))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to perform OCR", ex);
            }
        }
    }
}
