using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;
using System.Text.RegularExpressions;

public class GoogleLoginManager : MonoBehaviour
{
    [Header("Google API")]
    // Web Client ID'nizi buraya girin
    private string GoogleAPI = "403346372927-5enk77qjek6j1maje1toq6010umug62v.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("Firebase Auth")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    [Header("UI References")]
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject nicknamePanel;

    [Header("Nickname")]
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button nicknameSubmit;
    private string currentNickname = "";
    [SerializeField] private TMP_Text nicknameError;

    private bool isGoogleSignInInitialized = false;

    private void Start()
    {
        InitFirebase();

        nicknameInput.onValueChanged.AddListener(OnNicknameChanged);

        if(nicknameError != null)
        {
            nicknameError.gameObject.SetActive(false);
        }
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        auth.StateChanged += authStateChanged;
        authStateChanged(this, null);
    }

    void authStateChanged(object sender,System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if(signedIn && user != null)
            {
                Debug.Log("Oturum Kapatýldý");
            }
            user = auth.CurrentUser;
            if(signedIn)
            {
                Debug.Log($"Oturum Açýldý : {user.Email}");
                CheckUserInDatabase(user);
            }
        }
    }

    void CheckUserInDatabase(FirebaseUser newUser)
    {
        DocumentReference userDocRef = db.Collection("kullanicilar").Document(newUser.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Firestore'dan veri alýnamadý: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                PlayerPrefs.SetInt("IsLoggedIn", 1);
                PlayerPrefs.Save();

                GameStateManager.IsSigningOut = false;

                SceneManager.LoadScene("LoadingScene");
            }
            else
            {
                Debug.Log("Yeni kullanýcý tespit edildi. Nickname paneli açýlýyor.");
                LoginPanel.SetActive(false);
                nicknamePanel.SetActive(true);

                nicknameSubmit.onClick.RemoveAllListeners();
                nicknameSubmit.onClick.AddListener(() =>
                {
                    SaveNickname(newUser);
                });
            }
        });
    }
    public async void SaveNickname(FirebaseUser userToSave)
    {
        string nickname = currentNickname.Trim();

        if (nickname.Length < 3 || nickname.Length > 14)
        {
            ShowNicknameError("Nickname 3 ile 14 karakter arasýnda olmalýdýr.");
            return;
        }
        if(!Regex.IsMatch(nickname,@"^[a-zA-Z0-9]+$"))
        {
            ShowNicknameError("Nickname sadece harf ve rakam içerebilir.");
            return;
        }

        nicknameSubmit.interactable = false;

        Query query = db.Collection("kullanicilar").WhereEqualTo("nickname", nickname);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        if (querySnapshot.Count > 0)
        {
            ShowNicknameError("Bu nickname zaten alýnmýþ. Lütfen baþka bir tane dene.");
            nicknameSubmit.interactable = true;
            return;
        }

        DocumentReference userDocRef = db.Collection("kullanicilar").Document(userToSave.UserId);

        string defaultCharacterID = "civciv_01";

        var userData = new Dictionary<string, object>
        {
            {"email" , userToSave.Email},
            {"nickname" , nickname },
            {"eloPoints",0 },
            {"coins", 100 },
            {"ownedCharacterIDs", new List<string> { defaultCharacterID } },
            {"selectedCharacterID", defaultCharacterID  },
            {"friends",new List<string>() },
            {"created_at", FieldValue.ServerTimestamp },
            {"device_language",Application.systemLanguage.ToString() },
            {"country_code",RegionInfo.CurrentRegion.TwoLetterISORegionName },
            {"device_model",SystemInfo.deviceModel }
        };

        await userDocRef.SetAsync(userData);

        Debug.Log("Nickname baþarýyla kaydedildi! Oyun sahnesi yükleniyor...");
        nicknamePanel.SetActive(false);

        PlayerPrefs.SetInt("IsLoggedIn", 1);
        PlayerPrefs.Save();

        GameStateManager.IsSigningOut = false;

        SceneManager.LoadScene("LoadingScene");
    }

    public void OnNicknameChanged(string newText)
    {
        currentNickname = newText;

        if(nicknameError != null && nicknameError.gameObject.activeSelf)
        {
            nicknameError.gameObject.SetActive(false);
        }
    }

    private void ShowNicknameError(string message)
    {
        Debug.LogError(message);
        if (nicknameError != null)
        {
            nicknameError.text = message;
            nicknameError.gameObject.SetActive(true);
        }
    }

    public void Login()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };
            isGoogleSignInInitialized = true;
        }

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("Google sign-in was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Google sign-in encountered an error: " + task.Exception);
                return;
            }

            GoogleSignInUser googleUser = task.Result;
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled)
                {
                    Debug.LogWarning("Firebase auth was canceled.");
                    return;
                }

                if (authTask.IsFaulted)
                {
                    Debug.LogError("Firebase auth failed: " + authTask.Exception);
                    return;
                }
            });
        });
    }
    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= authStateChanged;
            auth = null;
            Debug.Log("GoogleLoginManager: Auth state listener temizlendi.");
        }
    }
}
