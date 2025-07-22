using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class FirebaseInitManager : MonoBehaviour
{
    [SerializeField] private Image firebaseAuthControllerImage;
    [SerializeField] private Color connectedColor, errorColor;
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                Debug.Log("<color=green>===========[FIREBASE CONNECTED ✅]===========</color>");
                firebaseAuthControllerImage.color = connectedColor;

                FirebaseApp app = FirebaseApp.DefaultInstance;
            }
            else
            {
                Debug.Log("<color=green>===========[FIREBASE FAILED ❌]===========</color>");
                firebaseAuthControllerImage.color=errorColor;
            }
        });
    }
}

