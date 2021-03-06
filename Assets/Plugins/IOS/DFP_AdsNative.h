// -------------------------------------------------------
//  Created by Andrew Witte.
//  Copyright (c) 2013 Reign-Studios. All rights reserved.
// -------------------------------------------------------

#import <Foundation/Foundation.h>
#import <GoogleMobileAds/GADBannerView.h>
#import <GoogleMobileAds/DFPBannerView.h>

@interface DFP_AdsNative : UIViewController<GADBannerViewDelegate>
{
@private
    DFPBannerView *bannerView;
    
@public
    BOOL testing, visible;
    NSMutableArray* events;
}

- (void)CreateAd:(int)gravity adSizeIndex:(int)adSizeIndex unitID:(char*)unitID;
- (void)SetGravity:(CGSize)viewSize gravity:(int)gravity;
- (void)SetVisible:(BOOL)visibleValue;
- (void)Refresh;
@end