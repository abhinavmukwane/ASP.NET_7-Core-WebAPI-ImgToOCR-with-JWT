namespace ImgOCR.Models
{
    public class OcrResponse
    {
        public int ResponseCode { get; set; }
        public ResponseData Response { get; set; }
        public string ErrorMsg { get; set; }
    }
}
