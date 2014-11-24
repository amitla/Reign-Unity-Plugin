// -------------------------------------------------------
//  Created by Andrew Witte.
//  Copyright (c) 2013 Reign-Studios. All rights reserved.
// -------------------------------------------------------

#import "GameCenterNative.h"
#import "UnityTypes.h"

@implementation ReignNativeGameCenter
- (id)init
{
    self = [super init];
    // init other data here...
    userAuthenticated = false;
    authenticateDone = false;
    authenticatedError = nil;
    userID = nil;
    
    reportScoreDone = false;
    reportAchievementError = nil;
    reportScoreError = nil;
    return self;
}

- (void)dealloc
{
    // dispose...
    [super dealloc];
}

- (void)authenticationChanged
{
    if ([GKLocalPlayer localPlayer].isAuthenticated)
    {
        userID = [[NSString alloc] initWithString:[GKLocalPlayer localPlayer].alias];
        NSLog(@"Authentication changed: player authenticated with user: %@", userID);
        authenticatedError = nil;
        userAuthenticated = true;
        authenticateDone = true;
    }
    else if (![GKLocalPlayer localPlayer].isAuthenticated)
    {
        NSLog(@"Authentication changed: player not authenticated.");
        userAuthenticated = false;
    }
}

- (void)SetCallbacks
{
    NSNotificationCenter *nc = [NSNotificationCenter defaultCenter];
    [nc addObserver:self selector:@selector(authenticationChanged) name:GKPlayerAuthenticationDidChangeNotificationName object:nil];
}

- (void)Authenticate
{
    NSLog(@"Authenticating local user...");
    if([GKLocalPlayer localPlayer].authenticated == false)
    {
        [[GKLocalPlayer localPlayer] authenticateWithCompletionHandler:^(NSError *error)
         {
             if (error != nil)
             {
                 NSLog(@"GameCenter Error: %@", error.localizedDescription);
                 authenticatedError = [[NSString alloc] initWithString:error.localizedDescription];
                 userAuthenticated = false;
                 authenticateDone = true;
             }
         }];
    }
}

- (void)ReportScore:(int64_t)score leaderboardID:(NSString*)leaderboardID
{
    GKScore* scoreReporter = [[[GKScore alloc] initWithCategory:leaderboardID] autorelease];
    scoreReporter.value = score;
    [scoreReporter reportScoreWithCompletionHandler: ^(NSError *error)
    {
        if (error != nil)
        {
            reportScoreError = [[NSString alloc] initWithString:error.localizedDescription];
            reportScoreSucceeded = false;
        }
        else
        {
            if (reportScoreError != nil)
            {
                [reportScoreError dealloc];
                reportScoreError = nil;
            }
            reportScoreSucceeded = true;
        }
        
        reportScoreDone = true;
    }];
}

- (void)ReportAchievement:(NSString*)achievementID percentComplete:(double)percentComplete
{
    GKAchievement* achievement = [[[GKAchievement alloc] initWithIdentifier:achievementID] autorelease];
    achievement.percentComplete = percentComplete;
    [achievement reportAchievementWithCompletionHandler:^(NSError *error)
     {
         if (error != nil)
         {
             reportAchievementError = [[NSString alloc] initWithString:error.localizedDescription];
             reportAchievementSucceeded = false;
         }
         else
         {
             if (reportAchievementError != nil)
             {
                 [reportAchievementError dealloc];
                 reportAchievementError = nil;
             }
             reportAchievementSucceeded = true;
         }
         
         reportAchievementDone = true;
     }];
}

- (void)leaderboardViewControllerDidFinish:(GKLeaderboardViewController *)viewController
{
    [UnityGetGLViewController() dismissModalViewControllerAnimated: YES];
    [viewController release];
}

- (void)ShowScoresPage:(NSString*)leaderboardID
{
    GKLeaderboardViewController* leaderboardController = [[GKLeaderboardViewController alloc] init];
    if (leaderboardController != NULL)
    {
        leaderboardController.category = leaderboardID;
        leaderboardController.timeScope = GKLeaderboardTimeScopeWeek;
        leaderboardController.leaderboardDelegate = self;
        [UnityGetGLViewController() presentModalViewController: leaderboardController animated: YES];
        [leaderboardID dealloc];
    }
}

- (void)achievementViewControllerDidFinish:(GKAchievementViewController *)viewController;
{
    [UnityGetGLViewController() dismissModalViewControllerAnimated: YES];
    [viewController release];
}

- (void)ShowAchievementsPage
{
    GKAchievementViewController *achievements = [[GKAchievementViewController alloc] init];
    if (achievements != NULL)
    {
        achievements.achievementDelegate = self;
        [UnityGetGLViewController() presentModalViewController: achievements animated: YES];
    }
}
@end

// ----------------------------------
// Unity C Link
// ----------------------------------
static ReignNativeGameCenter* native = nil;

extern "C"
{
    void InitGameCenter()
    {
        if (native == nil)
        {
            native = [[ReignNativeGameCenter alloc] init];
            [native SetCallbacks];
        }
    }
    
    void AuthenticateGameCenter()
    {
        native->authenticateDone = false;
        [native Authenticate];
    }
    
    bool GameCenterCheckAuthenticateDone()
    {
        if (native == nil) return false;
        return native->authenticateDone;
    }
    
    bool GameCenterCheckIsAuthenticated()
    {
        if (native == nil) return false;
        return native->userAuthenticated;
    }
    
    char* GameCenterGetAuthenticatedError()
    {
        if (native->authenticatedError == nil) return 0;
        const char* error = [native->authenticatedError cStringUsingEncoding:NSUTF8StringEncoding];
        return (char*)error;
    }
    
    char* GameCenterGetUserID()
    {
        const char* userID = [native->userID cStringUsingEncoding:NSUTF8StringEncoding];
        return (char*)userID;
    }
    
    void GameCenterReportScore(int score, const char* leaderboardID)
    {
        NSString* nativeID = [[NSString alloc] initWithUTF8String:leaderboardID];
        [native ReportScore:score leaderboardID:nativeID];
    }
    
    bool GameCenterReportScoreDone()
    {
        if (native == nil) return false;
        bool value = native->reportScoreDone;
        native->reportScoreDone = false;
        return value;
    }
    
    bool GameCenterReportScoreSucceeded()
    {
        if (native == nil) return false;
        return native->reportScoreSucceeded;
    }
    
    char* GameCenterReportScoreError()
    {
        if (native->reportScoreError == nil) return 0;
        const char* error = [native->reportScoreError cStringUsingEncoding:NSUTF8StringEncoding];
        return (char*)error;
    }
    
    void GameCenterReportAchievement(const char* achievementID)
    {
        NSString* nativeID = [[NSString alloc] initWithUTF8String:achievementID];
        [native ReportAchievement:nativeID percentComplete:100];
    }
    
    bool GameCenterReportAchievementDone()
    {
        if (native == nil) return false;
        bool value = native->reportAchievementDone;
        native->reportAchievementDone = false;
        return value;
    }
    
    bool GameCenterReportAchievementSucceeded()
    {
        if (native == nil) return false;
        return native->reportAchievementSucceeded;
    }
    
    char* GameCenterReportAchievementError()
    {
        if (native->reportAchievementError == nil) return 0;
        const char* error = [native->reportAchievementError cStringUsingEncoding:NSUTF8StringEncoding];
        return (char*)error;
    }
    
    void GameCenterShowScoresPage(const char* leaderboardID)
    {
        NSString* nativeID = [[NSString alloc] initWithUTF8String:leaderboardID];
        [native ShowScoresPage:nativeID];
    }
    
    void GameCeneterShowAchievementsPage()
    {
        [native ShowAchievementsPage];
    }
}