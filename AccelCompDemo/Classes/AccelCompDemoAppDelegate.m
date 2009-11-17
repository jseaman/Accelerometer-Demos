//
//  AccelCompDemoAppDelegate.m
//  AccelCompDemo
//
//  Created by Julio Seaman on 11/8/09.
//  Copyright FXL 2009. All rights reserved.
//

#import "AccelCompDemoAppDelegate.h"
#import "AccelCompDemoViewController.h"

@implementation AccelCompDemoAppDelegate

@synthesize window;
@synthesize viewController;


- (void)applicationDidFinishLaunching:(UIApplication *)application {    
    
    // Override point for customization after app launch    
    [window addSubview:viewController.view];
    [window makeKeyAndVisible];
}


- (void)dealloc {
    [viewController release];
    [window release];
    [super dealloc];
}


@end
