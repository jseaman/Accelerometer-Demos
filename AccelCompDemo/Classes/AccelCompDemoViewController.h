//
//  AccelCompDemoViewController.h
//  AccelCompDemo
//
//  Created by Julio Seaman on 11/8/09.
//  Copyright FXL 2009. All rights reserved.
//

#import <UIKit/UIKit.h>
#import<CoreLocation/CoreLocation.h>

@interface AccelCompDemoViewController : UIViewController <UIAccelerometerDelegate, CLLocationManagerDelegate> {
	UIButton *startButton;
	UIButton *stopButton;
	UILabel *connectedLabel;
	UITextField *ipTextField;
	
	CLLocationManager *location;
	
	UIActivityIndicatorView *activityView;
}

- (IBAction) startButtonPressed:  (id) sender;
- (IBAction) stopButtonPressed:  (id) sender;

- (IBAction) backgroundTap: (id) sender;

//- (void) changeButtonStatuses;

@property (nonatomic, retain) IBOutlet UIButton *startButton;
@property (nonatomic, retain) IBOutlet UIButton *stopButton;
@property (nonatomic, retain) IBOutlet UILabel *connectedLabel;
@property (nonatomic, retain) CLLocationManager *location;
@property (nonatomic, retain) IBOutlet UIActivityIndicatorView *activityView;
@property (nonatomic, retain) IBOutlet UITextField *ipTextField;

@end

