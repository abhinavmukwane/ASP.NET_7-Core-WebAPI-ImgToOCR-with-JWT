# ASP.NET_7-Core-WebAPI-ImgToOCR-with-JWT
ASP.NET_7-Core-WebAPI-ImgToOCR-with-JWT

To Generate Token call API:
/api/jwtTokenGen/getToken

Payload: 
{
    "partner_id" : "123456789101", --12 digits
    "txn_id": "22b83c2e-f6bf-474b-84fd-6ef10f918f8b", --guid
    "time_stamp": "2024-05-25T10:53:37.760Z"  --TimeStamp ISO 8601
}


To call Img TO OCR API:
/api/ocr/IMGtoOCR

Payload:
{
    "ImageBase64":"Your img base64 string"
}
Headers:
partner_id:123456789101
txn_id:22b83c2e-f6bf-474b-84fd-6ef10f918f8b
time_stamp:2024-05-25T10:53:37.760Z
token: Your generated token 
