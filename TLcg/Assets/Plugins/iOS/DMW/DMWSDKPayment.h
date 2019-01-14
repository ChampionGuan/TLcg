#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

@interface DMWSDKPayment : NSObject
// 支付
- (void)paySomething:(NSString *)cpOrderid :(NSString *)roleid :(NSString *)rolename :(NSString *)serverid :(NSString *)money
                    :(NSString *)gold :(NSString *)productid :(NSString *)productname :(NSString *)ext :(NSString *)test;

@end
