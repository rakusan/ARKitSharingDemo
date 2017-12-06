//
//  QRCodeReader.m
//  Unity-iPhone
//
//  Created by Yoshikazu Kuramochi on 2017/06/22.
//

#import <Foundation/Foundation.h>

static float qrcodeCorners[8];
static volatile BOOL reading = false;

void ReadQRCode(long long mtlTexPtr)
{
    if (reading) return;
    reading = YES;
    
    MTLTextureRef mtlTex = (__bridge MTLTextureRef) (void*) mtlTexPtr;
    CIImage *ciImage = [CIImage imageWithMTLTexture:mtlTex options:nil];
    
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        CGFloat iw = ciImage.extent.size.width;
        CGFloat ih = ciImage.extent.size.height;
        
        CIDetector *detector = [CIDetector detectorOfType:CIDetectorTypeQRCode context:nil options:nil];
        NSArray<CIFeature *> *features = [detector featuresInImage:ciImage];
        
        if (features.count > 0) {
            CIQRCodeFeature *feature = (CIQRCodeFeature*) [features objectAtIndex:0];
            
            // TODO シェアリング用のQRコードかどうかの識別を feature.messageString の内容で行う。
            
            qrcodeCorners[0] = feature.topLeft.x / iw;
            qrcodeCorners[1] = feature.topLeft.y / ih;
            qrcodeCorners[2] = feature.topRight.x / iw;
            qrcodeCorners[3] = feature.topRight.y / ih;
            qrcodeCorners[4] = feature.bottomLeft.x / iw;
            qrcodeCorners[5] = feature.bottomLeft.y / ih;
            qrcodeCorners[6] = feature.bottomRight.x / iw;
            qrcodeCorners[7] = feature.bottomRight.y / ih;
            
            UnitySendMessage("QRCodeReader", "OnReadQRCode", "");
        }
        
        reading = NO;
    });
}

void GetQRCodeCorners(int32_t **cornersPtr)
{
    float *floatArray = malloc(sizeof(float) * 8);
    memcpy(floatArray, qrcodeCorners, sizeof(qrcodeCorners));
    *cornersPtr = (int32_t*) floatArray;
}
