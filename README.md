# Twot

Is a simple CLI app tool to help with managing your Twitter experience by giving some advanced tools.

*Note:* This is really beta at this stage. Log an [incident](https://github.com/rmaclean/twot/issues) if you find issues.

## Requirement

You will need to register a [Twitter App](https://developer.twitter.com/en/apps) to get the four keys you will need to setup the tool.

### Set API Keys

Run the following commands setting the various keys:
- `dotnet user-secrets set "twot:apikey" "???"`
- `dotnet user-secrets set "twot:apisecret" "???"`
- `dotnet user-secrets set "twot:accesstoken" "???"`
- `dotnet user-secrets set "twot:accesssecret" "???"`

## Using

Execute twot followed by a command, currently there are two BlockTrain and Clean (they are case sensitive).

### Common parameters

Both commands accept a username, this should be yours which can be provided with `--username` or `-u` followed by your username (without the @ sign). The other command is dry run, which is specified with `--dryrun` which will let you see the changes without making them.

### BlockTrain

This will allow you to block a user, and all their followers. In addtion to the common parameters, you just need to specify the targets username without the @, using `--target` or `-t`.

#### People you follow
People who you follow will not be blocked in the block train.

### Clean

This will allow you to "force unfollow", block and unblock a person which causes them to unfollow you, your followers based on a score. The goal is to get rid of bots and abandoned accounts. You do not need to provide anything beyond the common parameters, however there is a score parameter, `--score`, to change how aggressive you want to be. 

#### People you follow
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
