# Twot: Making Twitter Better

[![License: Unlicense](https://img.shields.io/badge/license-Unlicense-blue.svg)](http://unlicense.org/) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/adaa49683a7e49cd8c243e641e6f8a66)](https://www.codacy.com/manual/rmaclean/twot?utm_source=github.com&utm_medium=referral&utm_content=rmaclean/twot&utm_campaign=Badge_Grade)

Is a simple CLI app tool to help with managing your Twitter experience by giving some advanced tools.

_Note:_ This is really beta at this stage. Log an [incident](https://github.com/rmaclean/twot/issues) if you find issues.

## Requirement

You will need to register a [Twitter App](https://developer.twitter.com/en/apps) to get the four keys you will need to setup the tool.

### Set API Keys

The API keys can be stored using the .NET user secret store or in a secrets.json file.

#### secrets.json

Creating a secrets.json file is the easist. Just place the following in, replacing the ??? with your keys you got above.

```
{
    "twot:apikey": "???",
    "twot:apisecret": "???",
    "twot:accesstoken": "???",
    "twot:accesssecret": "???"
}
```

#### Using User Secrets

Follow [this guide](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows#enable-secret-storage) to enable the user secret storage.

Run the following commands setting the various keys:

- `dotnet user-secrets set "twot:apikey" "???"`  
- `dotnet user-secrets set "twot:apisecret" "???"`  
- `dotnet user-secrets set "twot:accesstoken" "???"`  
- `dotnet user-secrets set "twot:accesssecret" "???"`  

## Using

Execute Twot followed by a command, currently there are three: Ready, BlockTrain and Clean (they are case sensitive).

### Common parameters

Both BlockTrain and Clean accept a dry run parameter, which is specified with `--dryrun` which will let you see the changes without actually making them.

### Ready

Running this command will tell you if you are correctly setup. This is useful to verify the API secrets.

### BlockTrain

This will allow you to block a user, and all their followers. In addtion to the common parameters, you just need to specify the targets username without the @, using `--target` or `-t`.

#### BlockTrain and people you follow

People who you follow will not be blocked in the block train.

### Clean

This will allow you to "force unfollow", block and unblock a person which causes them to unfollow you, your followers based on a score. The goal is to get rid of bots and abandoned accounts. You do not need to provide anything beyond the common parameters, however there is a score parameter, `--score`, to change how aggressive you want to be.

#### Clean and people you follow

People who you follow will not get unfollowed.

#### Scoring Rules

Each rule is applied to a follower and if they exceed the configuration they will be blocked/unblocked.

1. Default Profile Image: +1
2. No description: +0.3
3. No favourites: +0.8
4. Followers less than 20: +0.5
5. No location: +0.2
6. More than 10k followers: +0.2
7. More than 25k followers: +0.7 (stacks with 6)
8. Less than 10 tweets: +0.3
9. Zero tweets: +0.5 (stacks with 8)
10. Tweeted this week: -0.4
11. Tweeted more than 90 days ago: +0.5
12. Tweeted more than 365 days ago: +1
13. Follows more than 5k people: +0.3
