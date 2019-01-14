#import "DMWSDKPayment.h"
#import "CMPBusinessLogicServiceClass.h"

@interface DMWSDKPayment ()<CMPBusinessLogicServiceClassPaymentDelegate> //支付的代理

@end


@implementation DMWSDKPayment

// 支付
- (void)paySomething:(NSString *)cpOrderid :(NSString *)roleid :(NSString *)rolename :(NSString *)serverid :(NSString *)money
 :(NSString *)gold :(NSString *)productid :(NSString *)productname :(NSString *)ext :(NSString *)test {
    //支付需要的参数
    NSDictionary *params = @{
                             @"cp_order_id"    :   cpOrderid,
                             @"roleid"         :   roleid,
                             @"rolename"       :   rolename,
                             @"serverid"       :   serverid,
                             @"money"          :   money,
                             @"gold"           :   gold,
                             @"productid"      :   productid,
                             @"product_name"   :   productname,
                             @"ext"            :   ext,
                             @"test"           :   test  // 0或者1 0：支付回调正式地址 1：支付回调测试地址
                             };
    [[CMPBusinessLogicServiceClass sharedPlatform] paymentTransmitField:params];
    [CMPBusinessLogicServiceClass sharedPlatform].paymentDelegate = self;

}
///购买成功
- (void)CMPChannelPaymentSucceed:(NSInteger)value{
   NSLog(@"充值成功==%ld",value);
   
   NSString *msg = [NSString stringWithFormat:@"支付成功:%ld", (long)value];
   UIAlertView *view = [[UIAlertView alloc] initWithTitle:@"提示" message:msg delegate:self cancelButtonTitle:@"确定" otherButtonTitles:nil, nil];
   [view show];

    // 通知Unity支付成功
    UnitySendMessage("GameManager", "PaySucceed", "");
}
///购买失败
- (void)CMPChannelPaymentFailure:(NSString *)error{
   NSLog(@"充值失败==%@",error);
   NSString *msg = [NSString stringWithFormat:@"支付失败回调：%@", error];
   UIAlertView *view = [[UIAlertView alloc] initWithTitle:@"提示" message:msg delegate:self cancelButtonTitle:@"确定" otherButtonTitles:nil, nil];
   [view show];

    // 通知Unity支付失败
    UnitySendMessage("GameManager", "PayFailure", "");
}
///支付过程中,支付页面关闭
- (void)CMPChannelPaymentPageClosed{
   NSLog(@"支付界面关闭回调");
   UIAlertView *view = [[UIAlertView alloc] initWithTitle:@"提示" message:@"支付页面关闭" delegate:self cancelButtonTitle:@"确定" otherButtonTitles:nil, nil];
   [view show];

    // 通知Unity支付失败
    UnitySendMessage("GameManager", "PayFailure", "");
}

@end
