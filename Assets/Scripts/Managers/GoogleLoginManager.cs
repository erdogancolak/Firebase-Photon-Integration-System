using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GoogleLoginManager : MonoBehaviour
{
    [Header("Google API")]
    // Web Client ID'nizi buraya girin
    private string GoogleAPI = "403346372927-5enk77qjek6j1maje1toq6010umug62v.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("Firebase Auth")]
    private FirebaseAuth auth;
    private FirebaseUser user;

    [Header("UI References")]
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject UserPanel;
    [SerializeField] private TMP_Text UserEmail;
    [SerializeField] private TMP_Text Username;
    [SerializeField] private Image UserProfilePic;
    [SerializeField] private Image checkNewUser;
    [SerializeField] private Color newUserColor;
    [SerializeField] private Color defaultUserColor = Color.white; // Varsayýlan renk

    private string imageUrl;
    private bool isGoogleSignInInitialized = false;

    private void Start()
    {
        InitFirebase();
        // Baþlangýçta kontrol rengini varsayýlana ayarla
        if (checkNewUser != null)
        {
            checkNewUser.color = defaultUserColor;
        }
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
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

                // --- DÜZELTME: AuthResult kaldýrýldý, doðrudan FirebaseUser kullanýlýyor ---
                // SignInWithCredentialAsync'in sonucu artýk doðrudan bir FirebaseUser'dýr.
                user = authTask.Result;
                // --- DÜZELTME SONU ---

                // Kullanýcýnýn metadata'sýndan oluþturulma ve son giriþ zamanlarýný alýyoruz.
                ulong creationTimestamp = user.Metadata.CreationTimestamp;
                ulong lastSignInTimestamp = user.Metadata.LastSignInTimestamp;

                // Eðer son giriþ zamaný ile oluþturulma zamaný arasýndaki fark çok küçükse
                // (örneðin 2 saniyeden az), bu kullanýcýnýn ilk giriþi demektir.
                if (lastSignInTimestamp - creationTimestamp < 2000)
                {
                    Debug.Log("Yeni kullanýcý zaman damgasý karþýlaþtýrmasý ile tespit edildi!");
                    checkNewUser.color = newUserColor;
                }
                else
                {
                    Debug.Log("Mevcut kullanýcý tespit edildi.");
                    checkNewUser.color = defaultUserColor;
                }

                Username.text = user.DisplayName;
                UserEmail.text = user.Email;

                LoginPanel.SetActive(false);
                UserPanel.SetActive(true);

                StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl?.ToString())));
            });
        });
    }

    private string CheckImageUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        return imageUrl;
    }

    IEnumerator LoadImage(string imageUri)
    {
        if (string.IsNullOrEmpty(imageUri)) yield break; // URL boþsa iþlemi bitir

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUri))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (UserProfilePic != null)
                {
                    UserProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log("Image loaded successfully.");
                }
            }
            else
            {
                Debug.LogError("Error loading profile image: " + www.error);
            }
        }
    }

    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
        }
        GoogleSignIn.DefaultInstance.SignOut();

        LoginPanel.SetActive(true);
        UserPanel.SetActive(false);

        // Çýkýþ yapýldýðýnda kontrol rengini varsayýlana sýfýrla
        if (checkNewUser != null)
        {
            checkNewUser.color = defaultUserColor;
        }
    }
}
