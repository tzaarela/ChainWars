using Assets.Scripts.Models;
using Firebase;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json;
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
        public FirebaseDatabase root;

        public Action onUserRegistered;
        public Action onUserSignedIn;
        public Action onSignInFailed;
        public Action onRegisterFailed;
        public Action onLobbyCreated;
        public Action<List<Lobby>> onLobbiesRefreshed;
        public Action onFirebaseInitialized;

        public void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {

                    app = FirebaseApp.DefaultInstance;
                    auth = FirebaseAuth.DefaultInstance;
                    root = FirebaseDatabase.DefaultInstance;
                    Debug.Log("firebase initialized");
                    onFirebaseInitialized();
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

                user = newUser;

                var lobbyPlayer = new LobbyPlayer();
                lobbyPlayer.email = newUser.Email;
                lobbyPlayer.username = username;
                lobbyPlayer.playerId = Guid.NewGuid().ToString();

                string jsonValue = JsonConvert.SerializeObject(lobbyPlayer);

                root.RootReference.Child("players").Child(newUser.UserId).SetRawJsonValueAsync(jsonValue).ContinueWithOnMainThread(task =>
                {
                    Debug.Log("Created db reference to user");

                    onUserRegistered();
                });
            });
        }

        public void SignInUser(string email, string password)
        {
            auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
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

                onUserSignedIn?.Invoke();
            });
        }

        public void LogOut()
		{
            auth.SignOut();
		}

        public async Task<Lobby> CreateLobbyAsync(string name)
        {
            var dbRef = root.RootReference.Child("lobbies").Push();
            
            Lobby lobby = new Lobby(name);
            lobby.lobbyId = dbRef.Key;

            string jsonValue = JsonConvert.SerializeObject(lobby);
            await dbRef.SetRawJsonValueAsync(jsonValue).ContinueWithOnMainThread(task =>
            {
                Debug.Log("Created new lobby");
            });

            return lobby;
        }

        public Task RemoveLobby(string id)
		{
            return root.RootReference.Child("lobbies").Child(id).RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError(task.Exception);

                Debug.Log("Lobby Removed");
            });
        }

        public async Task<List<Lobby>> GetLobbyListAsync()
        {
            var lobbies = new List<Lobby>();
            await root.GetReference("lobbies").GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError(task.Exception);// Handle the error...
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;

                    foreach (var lobby in snapshot.Children)
                    {
                        var rawJson = lobby.GetRawJsonValue();
                        lobbies.Add(JsonConvert.DeserializeObject<Lobby>(rawJson));
                    }
                }
            });
            return lobbies;
        }
    }
}
