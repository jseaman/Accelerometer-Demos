//
//  AccelCompDemoViewController.m
//  AccelCompDemo
//
//  Created by Julio Seaman on 11/8/09.
//  Copyright FXL 2009. All rights reserved.
//

#import "AccelCompDemoViewController.h"

#import <netinet/in.h>
#import <sys/socket.h>
#import <arpa/inet.h>

@implementation AccelCompDemoViewController

@synthesize startButton;
@synthesize stopButton;
@synthesize connectedLabel;
@synthesize location;
@synthesize activityView;
@synthesize ipTextField;

#define DATAFILE @"acceldemo.plist"

CFSocketRef sock;
uint16_t port = 666;

void sendVectorToSocket (double x, double y, double z, CFSocketRef socket);
void sendAnglesToSocket (double angle, double accuracy, CFSocketRef socket);

- (NSString *) dataFilePath {
	NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
	NSString *docDir = [paths objectAtIndex:0];
	return [docDir stringByAppendingPathComponent:DATAFILE];
}

// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
- (void)viewDidLoad {
    [super viewDidLoad];
	stopButton.enabled = NO;
	
	UIApplication *app = [UIApplication sharedApplication];
	app.idleTimerDisabled = YES;
	
	NSString *filePath = [self dataFilePath];
	
	if ([[NSFileManager defaultManager] fileExistsAtPath:filePath])
		ipTextField.text = [[NSString alloc] initWithContentsOfFile:filePath];
		
	[[NSNotificationCenter defaultCenter] addObserver: self selector: @selector(applicationWillTerminate:) name: UIApplicationWillTerminateNotification object:app];
}

- (void) showMessage: (NSString *) msg {
	UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Message" message:msg delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
	[alert show];
	[alert release];
}

static void connectCallback (CFSocketRef socket, CFSocketCallBackType type, CFDataRef address, const void *dataIn, void *info) {
	AccelCompDemoViewController *controller = (AccelCompDemoViewController *) info;
	
	if (dataIn)
	{
		SInt32 error = *((SInt32 *) dataIn);
		[controller showMessage:[NSString stringWithFormat:@"Connection error: %d",error]];
		[controller stopButtonPressed:nil];
		return;
	}
	
	[controller.activityView stopAnimating];
	controller.connectedLabel.hidden = NO;
	//[controller showMessage: @"me conecte!!!!"];
	
	controller.location = [[[CLLocationManager alloc] init] autorelease];
	
	if (controller.location.headingAvailable!=NO)
	{
		controller.location.delegate = controller;
		
		[controller.location startUpdatingHeading];
	}
	else 
	{
		[controller showMessage:@"compass NOT enabled"];
	}
	
	UIAccelerometer *accel = [UIAccelerometer sharedAccelerometer];
	accel.updateInterval = 1.0f/100.0f; 
	accel.delegate = controller;
}

- (CFSocketRef) initSocket {
	CFSocketContext context = {
		.version = 0,
		.info = self,
		.retain = NULL,
		.release = NULL,
		.copyDescription = NULL
	};
	
	CFSocketRef socket =  CFSocketCreate(
			kCFAllocatorDefault,
			PF_INET,
			SOCK_STREAM,
			IPPROTO_TCP,
			kCFSocketConnectCallBack,
			connectCallback,
			&context
	);
	
	struct sockaddr_in addr4;
	
	memset(&addr4,0, sizeof(addr4));
	addr4.sin_family = AF_INET;
	addr4.sin_len = sizeof(addr4);
	addr4.sin_port = htons(port);
	
	inet_aton([ipTextField.text UTF8String],&addr4.sin_addr);
	
	NSData *address = [NSData dataWithBytes:&addr4 length:sizeof(addr4)];
	
	CFSocketError sock_status = CFSocketConnectToAddress(socket, (CFDataRef)address, -1);
	NSLog(@"sock_status = %d",sock_status);
	
	CFRunLoopSourceRef source;
	source = CFSocketCreateRunLoopSource(NULL, socket, 1);
	CFRunLoopAddSource(CFRunLoopGetCurrent(), source, kCFRunLoopDefaultMode);
	CFRelease(source);

	return socket;
}

void sendVectorToSocket (double x, double y, double z, CFSocketRef socket) {
	Byte *b = (Byte *) malloc(25);
	Byte *p = b;
	
	char type='A';
	
	memcpy(p, &type, 1);
	p++;
	memcpy(p, &x, 8);
	p+=8;
	memcpy(p, &y, 8);
	p+=8;
	memcpy(p, &z, 8);

	NSData *d = [NSData dataWithBytes:b length:25];
	
	CFSocketError commError = CFSocketSendData(socket, NULL, (CFDataRef) d, 0);
	NSLog(@"comm error : %d",commError);

	free(b);
}

void sendAnglesToSocket (double angle, double accuracy, CFSocketRef socket) {
	Byte *b = (Byte *) malloc(17);
	Byte *p = b;
	
	char type='C';
	
	memcpy(p, &type, 1);
	p++;
	memcpy(p, &angle, 8);
	p+=8;
	memcpy(p, &accuracy, 8);
	
	NSData *d = [NSData dataWithBytes:b length:17];
	
	CFSocketError commError = CFSocketSendData(socket, NULL, (CFDataRef) d, 0);
	NSLog(@"comm error : %d",commError);
	
	free(b);
}

- (void) changeButtonStatuses {
	if (startButton.enabled)
		startButton.alpha = 0.5;
	else 
		startButton.alpha = 1;
	
	if (stopButton.enabled)
		stopButton.alpha = 0.5;
	else 
		stopButton.alpha = 1;
		
	startButton.enabled = !startButton.enabled;
	stopButton.enabled = !stopButton.enabled;
}

- (IBAction) startButtonPressed:  (id) sender {
	[activityView startAnimating];
	sock = [self initSocket];
	
	[ self changeButtonStatuses];
}

- (IBAction) stopButtonPressed:  (id) sender {
	CFSocketInvalidate(sock);
	connectedLabel.hidden=YES;
	[activityView stopAnimating];
	
	[location stopUpdatingHeading];
	UIAccelerometer *accel = [UIAccelerometer sharedAccelerometer];
	accel.delegate = nil;
	
	[self changeButtonStatuses];
}

- (IBAction) backgroundTap: (id) sender {
	[ipTextField resignFirstResponder];
}

- (void)accelerometer:(UIAccelerometer *)accelerometer didAccelerate:(UIAcceleration *)acceleration {
	sendVectorToSocket(acceleration.x, acceleration.y, acceleration.z, sock);
}

- (void)locationManager:(CLLocationManager *)manager
       didUpdateHeading:(CLHeading *)newHeading {
	sendAnglesToSocket(newHeading.magneticHeading, newHeading.headingAccuracy, sock);
}

- (BOOL)locationManagerShouldDisplayHeadingCalibration:(CLLocationManager *)manager {
	return YES;
}

- (void)locationManager:(CLLocationManager *)manager
	   didFailWithError:(NSError *)error {
	UIAlertView *Alert = [[UIAlertView alloc] initWithTitle:@"" message:error.localizedDescription delegate:nil cancelButtonTitle:@"Ok" otherButtonTitles:nil];
	[Alert show];
	[Alert release];
}

- (void) applicationWillTerminate: (NSNotification *) notification {
	[ipTextField.text writeToFile:[self dataFilePath] atomically:TRUE encoding: NSASCIIStringEncoding error: NULL];
}

- (void)didReceiveMemoryWarning {
	// Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
	
	// Release any cached data, images, etc that aren't in use.
}

- (void)viewDidUnload {
	// Release any retained subviews of the main view.
	// e.g. self.myOutlet = nil;
	startButton = stopButton = nil;
	connectedLabel = nil;
	activityView = nil;
	location = nil;
	ipTextField = nil;
		
	[UIApplication sharedApplication].idleTimerDisabled = NO;
}


- (void)dealloc {
	[startButton release];
	[stopButton release];
	[location release];
	[connectedLabel release];
	[activityView release];
	[ipTextField release];
	
	[UIApplication sharedApplication].idleTimerDisabled = NO;

    [super dealloc];
}

@end
