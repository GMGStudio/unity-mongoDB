
using System.Collections.Generic;
using System.Threading.Tasks;
using Realms;
using Realms.Sync;
using UnityEngine;
using Realms.Logging;
using Logger = Realms.Logging.Logger;
using Realms.Sync.ErrorHandling;
using Realms.Sync.Exceptions;
using System.Linq;
using System;

public class RealmController
{
    private Realm realm;
    private readonly string myRealmAppId = "app-XXXXXXX";
    private readonly string apiKey = "XXXXXXXXXXXXXXXXXXXXXXX";

    public RealmController()
    {
        InitAsync();
    }

    private async void InitAsync()
    {
        var app = App.Create(myRealmAppId);
        User user = await Get_userAsync(app);
        PartitionSyncConfiguration config = GetConfig(user);
        realm = await Realm.GetInstanceAsync(config);
    }

    private PartitionSyncConfiguration GetConfig(User user)
    {
        PartitionSyncConfiguration config = new("RocketGame", user);

        config.ClientResetHandler = new DiscardLocalResetHandler()
        {
            ManualResetFallback = (ClientResetException clientResetException) => clientResetException.InitiateClientReset()
        };
        return config;
    }

    private async Task<User> Get_userAsync(App app)
    {
        User user = app.CurrentUser;
        if (user == null)
        {
            user = await app.LogInAsync(Credentials.ApiKey(apiKey));
        }
        return user;
    }

    public void Terminate()
    {
        realm?.Dispose();
    }

    public void AddHighscore(string playerName, int score)
    {
        Highscore currentHighscore = null;
        if (realm == null)
        {
            Debug.Log("Realm not ready");
            return;
        }
        try {
            currentHighscore = realm.All<Highscore>().Where(highscore => highscore.Player == playerName).First();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        realm.Write(() =>
        {
            if (currentHighscore == null)
            {
                realm.Add(new Highscore()
                {
                    Player = playerName,
                    Score = score
                });
            }
            else
            {
                if (currentHighscore.Score < score)
                {
                    currentHighscore.Score = score;
                }
            }
        });
    }
}