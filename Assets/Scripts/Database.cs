using Assets.Scripts.Models;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Database
    {
        public FirebaseApp app;
        public FirebaseAuth auth;
        public FirebaseUser user;
        public FirebaseDatabase dbContext;

        public Action onUserRegistered;
        public Action onUserSignedIn;
        public Action onSignInFailed;
        public Action onRegisterFailed;
        public Action onLobbyCreated;
        public Action<List<Lobby>> onLobbiesRefreshed;
        public Action onFirebaseInitialized;

        public Database()
        {
        }

        public void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {

                    app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                    dbContext = FirebaseDatabase.DefaultInstance;

                    Dispatcher.RunOnMainThread(onFirebaseInitialized);
                }
                else
                {
                    Debug.LogError(System.String.Format(
                      "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                    // Firebase Unity SDK is not safe to use here.
                }
            });

        }

        public void RegisterUser(string username, string email, string password, string passwordVerify)
        {
            if(!password.Equals(passwordVerify))
			{
                Debug.Log("Password verification not matched");
                return;
			}

            auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);

                var lobbyPlayer = new LobbyPlayer();
                lobbyPlayer.email = newUser.Email;
                lobbyPlayer.username = username;

                string jsonValue = JsonUtility.ToJson(lobbyPlayer);

                dbContext.RootReference.Child("players").Child(newUser.UserId).SetRawJsonValueAsync(jsonValue).ContinueWith(task =>
                {
                    Debug.Log("Created db reference to user");

                    Dispatcher.RunOnMainThread(onUserRegistered);
                });
            });
        }

        public void SignInUser(string email, string password)
        {
            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                user = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    user.DisplayName, user.UserId);

                Dispatcher.RunOnMainThread(onUserSignedIn);
            });
        }

        public Lobby CreateLobby(string name)
        {
            Lobby lobby = new Lobby(name);
            string jsonValue = JsonUtility.ToJson(lobby);
            dbContext.RootReference.Child("Lobbies").Child(lobby.guid.ToString()).SetRawJsonValueAsync(jsonValue).ContinueWith(task =>
            {
                Debug.Log("Created new lobby");

                onLobbyCreated();
            });

            return lobby;
        }

        public void RemoveLobby(Guid guid)
		{

		}

        public List<Lobby> RefreshLobbies()
        {
            var lobbies = new List<Lobby>();

            dbContext.GetReference("Lobbies").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);// Handle the error...
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("refreshing lobby from database");
                    DataSnapshot snapshot = task.Result;


                    foreach (var lobby in snapshot.Children)
                    {
                        var rawJson = lobby.GetRawJsonValue();
                        lobbies.Add(JsonUtility.FromJson<Lobby>(rawJson));
                    }

                    onLobbiesRefreshed(lobbies);// Do something with snapshot...
                }
            });
            return lobbies;
        }
    }
}
