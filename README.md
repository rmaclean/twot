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

Execute Twot followed by a command, currently there are a number of commands:
- Ready: This helps check your configuration
- Init: This helps setup config files for you
- BlockTrain: This blocks a Twitter user and all their followers
- Clean: This kicks people from following you (uses block and then unblock), based on a score

### Common parameters

Both BlockTrain and Clean accept a dry run parameter, which is specified with `--dryrun` which will let you see the changes without actually making them.

### Ready

Running this command will tell you if you are correctly setup. This is useful to verify the API secrets.

#### Synopsis
<pre><strong>twot ready</strong></pre>


### Init

Init helps setup the environment. You can create a `secret.json` by passing in `secrets`, for the configuration for Twitter. You can create a `score.json` by passing in `score`, for use with the Clean command.

#### Synopsis
<pre><strong>twot init</strong> [score [--file &lt;filename&gt;]] [secrets [--file &lt;filename&gt;]]</pre>

### BlockTrain

This will allow you to block a user, and all their followers. In addtion to the common parameters, you just need to specify the targets username without the @, using `--target` or `-t`.

#### Synopsis
<pre><strong>twot BlockTrain</strong> [--dryrun] [--log | l] [--target | -t &lt;username&gt;]]</pre>

#### BlockTrain and people you follow

People who you follow will not be blocked in the block train.

### Clean

This will allow you to "force unfollow", block and unblock a person which causes them to unfollow you, your followers based on a score. The goal is to get rid of bots and abandoned accounts. You do not need to provide anything beyond the common parameters, however there is a score parameter, `--score`, to change how aggressive you want to be.

#### Synopsis
<pre><strong>twot Clean</strong> [--dryrun] [--log | l] [--score | -s &lt;min score&gt;]</pre>

### Score

This runs the same logic as *Clean* but does not make any actions and anyone who is below the score will be logged to the file. Logging is not optional on this one.

#### Synopsis
<pre><strong>twot Score</strong> [--score | -s &lt;min score&gt;]]</pre>

### Unblock

The unblock command will unblock a single person (with the *-t* parameter), a group of people from a file, such as the log file produced with *BlockTrain* and *Clean* (with the *-f* parameter) or everyone you have blocked (with the *--all* parameter).

#### Synopsis
<pre><strong>twot unblock</strong> [--dryrun] [--target | -t &lt;username&gt;] [--all] [--file | -f &lt;file&gt;]</pre>


## Cleaning, blocking and people you follow

People who you follow will not get unfollowed or blocked by the commands.

## Scoring Rules

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

#### Overriding Scoring Rules
If you do not like the scoring rules, they can be overriden by creating a file called `score.json` and changing the values in it.

Each item in the config is added as follows:
```json
  "Key": {
    "Enabled": true,
    "Value": true,
    "Impact": 1.0
  },
```

Break down:
- `Key` refers to a unique key for each setting. Only the keys which you are changing need to be specified but all three of the properties in each key must be specified for all supplied keys.
- `Enabled` is true or false to enable the rule.
- `Value` is what to use in the compare
- `Impact` this is how much of an impact it has on the score.

##### Examples

Here are some examples of making changes

###### Make it so having the default impact image to be 1 tenth the default
```json
{
  "DefaultProfileImage": {
    "Enabled": true,
    "Value": true,
    "Impact": 0.1
  }
}
```

###### Make it so NOT having the default impact image is bad
```json
{
  "DefaultProfileImage": {
    "Enabled": true,
    "Value": false,
    "Impact": 1.0
  }
}
```

###### Disable the rules for large/corporate accounts
```json
{
  "FollowersLarge": {
    "Enabled": false,
    "Value": 10000,
    "Impact": 0.2
  },
  "FollowersExtraLarge": {
    "Enabled": false,
    "Value": 25000,
    "Impact": 0.7
  },
  "FollowingMoreThan": {
    "Enabled": false,
    "Value": 5000,
    "Impact": 0.3
  }
}
```


##### Default score.json

You can also run `init score` to generate this.

```json
{
  "DefaultProfileImage": {
    "Enabled": true,
    "Value": true,
    "Impact": 1.0
  },
  "DescriptionLength": {
    "Enabled": true,
    "Value": 0,
    "Impact": 0.3
  },
  "Favourites": {
    "Enabled": true,
    "Value": 0,
    "Impact": 0.8
  },
  "FriendsLessThan": {
    "Enabled": true,
    "Value": 20,
    "Impact": 0.5
  },
  "LocationLength": {
    "Enabled": true,
    "Value": 0,
    "Impact": 0.2
  },
  "FollowersLarge": {
    "Enabled": true,
    "Value": 10000,
    "Impact": 0.2
  },
  "FollowersExtraLarge": {
    "Enabled": true,
    "Value": 25000,
    "Impact": 0.7
  },
  "TweetsLessThan": {
    "Enabled": true,
    "Value": 10,
    "Impact": 0.3
  },
  "ZeroTweets": {
    "Enabled": true,
    "Value": 0,
    "Impact": 0.5
  },
  "TweetedInLastWeek": {
    "Enabled": true,
    "Value": true,
    "Impact": -0.4
  },
  "TweetedMoreThan90Days": {
    "Enabled": true,
    "Value": true,
    "Impact": 0.5
  },
  "TweetedMoreThan1Year": {
    "Enabled": true,
    "Value": true,
    "Impact": 1.0
  },
  "FollowingMoreThan": {
    "Enabled": true,
    "Value": 5000,
    "Impact": 0.3
  }
}
```
