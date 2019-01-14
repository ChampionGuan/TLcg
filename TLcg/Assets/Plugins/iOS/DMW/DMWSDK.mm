#import "DMWSDK.h"
#import "DMWSDKPayment.h"
#import "CMPBusinessLogicServiceClass.h"
#import "CMPUserInformationModelParse.h"

@interface DMWSDK ()<CMPBusinessLogicServiceClassDelegate> //登录消息回调代理

@end


@implementation DMWSDK

//登录
- (void)login {
    [[CMPBusinessLogicServiceClass sharedPlatform] loginRequest];
    [CMPBusinessLogicServiceClass sharedPlatform].delegate = self;
}
/*登录回调接口*/
- (void)CMPChannelLoginInformationCallback {
    if ([[CMPBusinessLogicServiceClass sharedPlatform] isAvailableUserInformation]) 
    {
        NSString *isTemp = @"False";
        if([CMPUserInformationModelParse isTemporaryAccount])
            isTemp = @"True";
        NSString *userInfo = [NSString stringWithFormat:@"%@,%@,%@,%@", [CMPUserInformationModelParse GetCallbackUid], [CMPUserInformationModelParse GetCallbackAccont], [CMPUserInformationModelParse GetCallbackSessionid], isTemp];
        
        // invoke the unity's function
        // 平台用户信息
        UnitySendMessage("GameManager", "UpdatePlatformUserInfo", [userInfo UTF8String]);
    }
    else
    {
        UnitySendMessage("GameManager", "UpdatePlatformUserInfo", nil);
    }
}

// 登录成功后，上传游戏用户信息
- (void)uploadGameAccountInfo:(NSString *)sid : (NSString *)roleid : (NSString *)rolename : (NSString *)level : (NSString *)gold {
//    @"sid"      : @"服务器id",
//    @"roleid"   : @"角色id",
//    @"rolename" : @"角色名",
//    @"level"    : @"20",
//    @"gold"     : @"10000砖石",
    NSDictionary *gamerInfo = @{
                               @"sid"      : sid,
                               @"roleid"   : roleid,
                               @"rolename" : rolename,
                               @"level"    : level,
                               @"gold"     : gold,
                               };
    [[CMPBusinessLogicServiceClass sharedPlatform] AfterSuccessfulLoginUploadData:gamerInfo];
}

//    切换张号
- (void)switchAccount {
    [[CMPBusinessLogicServiceClass sharedPlatform] switchingLoginAccountNumber];
    [CMPBusinessLogicServiceClass sharedPlatform].delegate = self;
}

//登录之后调用此接口,显示SDK滚动条
- (void)showNotice {
    [[CMPBusinessLogicServiceClass sharedPlatform] ShowScrollViewBar];
}

- (void)logout {

}

@end

//////////////////////////////////////////////

// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;

    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

static DMWSDK* sdk = nil;
static DMWSDKPayment* pay = nil;


//////////////////////////////////////////////
// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
	void _login ()
	{
        if(sdk == nil)
            sdk = [[DMWSDK alloc] init];
        
        if(pay == nil)
            pay = [[DMWSDKPayment alloc] init];
        
        [sdk login];
	}
	
    void _switchAccount ()
    {
        [sdk switchAccount];
    }

    void _uploadGameAccountInfo(const char* sid, const char* roleid, const char* rolename, const char* level, const char* gold)
    {
        [sdk uploadGameAccountInfo :CreateNSString(sid) :CreateNSString(roleid) :CreateNSString(rolename) :CreateNSString(level) :CreateNSString(gold)];
    }
    
    void _showNotice ()
    {
        [sdk showNotice];
    }
    
    void _logout ()
    {
        
    }
    
     void _paySomething(const char* cpOrderid, const char* roleid, const char* rolename, const char* serverid, const char* money,
         const char* gold, const char* productid, const char* productname, const char* ext, const char* test)
     {
         [pay paySomething :CreateNSString(cpOrderid) :CreateNSString(roleid) :CreateNSString(rolename) :CreateNSString(serverid) :CreateNSString(money)
         :CreateNSString(gold) :CreateNSString(productid) :CreateNSString(productname) :CreateNSString(ext) :CreateNSString(test)];
     }
}
