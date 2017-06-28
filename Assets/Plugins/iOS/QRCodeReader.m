//
//  QRCodeReader.m
//  Unity-iPhone
//
//  Created by Yoshikazu Kuramochi on 2017/06/22.
//

#import <Foundation/Foundation.h>

static float qrcodeBounds[8];
static volatile BOOL reading = false;

void ReadQRCode(long long mtlTexPtr)
{
    if (reading) return;
    reading = YES;
    
    MTLTextureRef mtlTex = (__bridge MTLTextureRef) (void*) mtlTexPtr;
    CIImage *ciImage = [CIImage imageWithMTLTexture:mtlTex options:nil];
    
    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
        CGRect screenBounds = UIScreen.mainScreen.bounds;
        CGFloat screenScale = UIScreen.mainScreen.scale;
        CGFloat sw = screenBounds.size.width * screenScale;
        CGFloat sh = screenBounds.size.height * screenScale;
        CGFloat iw = ciImage.extent.size.width;
        CGFloat ih = ciImage.extent.size.height;
        
        CIDetector *detector = [CIDetector detectorOfType:CIDetectorTypeQRCode context:nil options:nil];
        NSArray<CIFeature *> *features = [detector featuresInImage:ciImage];
        
        [features enumerateObjectsUsingBlock:^(CIFeature * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {
            CIQRCodeFeature *feature = (CIQRCodeFeature*) obj;
            
            // TODO シェアリング用のQRコードかどうかの識別を feature.messageString の内容で行う。
            
            // スクリーン座標（縦向き）に変換しているが、いろいろ雑。
            qrcodeBounds[0] = sw - feature.topLeft.y     * sw / ih;
            qrcodeBounds[1] = sh - feature.topLeft.x     * sh / iw;
            qrcodeBounds[2] = sw - feature.topRight.y    * sw / ih;
            qrcodeBounds[3] = sh - feature.topRight.x    * sh / iw;
            qrcodeBounds[4] = sw - feature.bottomRight.y * sw / ih;
            qrcodeBounds[5] = sh - feature.bottomRight.x * sh / iw;
            qrcodeBounds[6] = sw - feature.bottomLeft.y  * sw / ih;
            qrcodeBounds[7] = sh - feature.bottomLeft.x  * sh / iw;
            
            UnitySendMessage("QRCodeReader", "OnReadQRCode", "");
            
            stop = YES;
        }];
        
        reading = NO;
    });
}

void GetQRCodeBounds(int32_t **boundsPtr)
{
    float *floatArray = malloc(sizeof(float) * 8);
    memcpy(floatArray, qrcodeBounds, sizeof(qrcodeBounds));
    *boundsPtr = (int32_t*) floatArray;
}
