//
//  AccelCompDemoAppDelegate.h
//  AccelCompDemo
//
//  Created by Julio Seaman on 11/8/09.
//  Copyright FXL 2009. All rights reserved.
//

#import <UIKit/UIKit.h>

@class AccelCompDemoViewController;

@interface AccelCompDemoAppDelegate : NSObject <UIApplicationDelegate> {
    UIWindow *window;
    AccelCompDemoViewController *viewController;
}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet AccelCompDemoViewController *viewController;

@end

