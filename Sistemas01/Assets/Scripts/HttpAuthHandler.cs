using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HttpAuthHandler : MonoBehaviour
{
    [SerializeField] private string ServerApiURL;
    [SerializeField] List<GameObject> objectosLogin = new List<GameObject>();
    [SerializeField] List<GameObject> objectosUsuario = new List<GameObject>();
    [SerializeField] TMP_Text userText;
    [SerializeField] TMP_Text scoreText;
    List<User> listaUsuarios = new List<User>();
    [SerializeField] List<TMP_Text> listaPuntajes = new List<TMP_Text>();
    [SerializeField] List<TMP_Text> listaNombres = new List<TMP_Text>();
    List<int> valoresPuntajes = new List<int>(5);

    public string Token { get; set; }
    public string Username { get; set; }

    void Start() {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(Token)) {
            Debug.Log("No hay token");
            //Ir a Login
        } else {
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetPerfil());
        }
    }
    public void Registrar() {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;
        string postData = JsonUtility.ToJson(user);
        listaUsuarios.Add(user);
        StartCoroutine(Registro(postData));
    }

    public void Ingresar() {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;
        string postData = JsonUtility.ToJson(user);
        listaUsuarios.Add(user);
        StartCoroutine(Login(postData));
    }
    
    IEnumerator Registro(string postData) {
        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios",postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " se regitro con id " + jsonData.usuario._id);
                LoginScreen(); //Proceso de autenticacion
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator Login(string postData) {
        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " inicio sesion");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);
                LoginScreen();
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator GetPerfil() {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");
                LoginScreen(); //Cambiar de pantalla
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    public void LoginScreen() {
        for (int i = 0; i < objectosLogin.Count; i++) {
            objectosLogin[i].SetActive(false);
        }
        for (int i = 0; i < objectosUsuario.Count; i++) {
            objectosUsuario[i].SetActive(true);
        }
        userText.text = "Welcome, " + Username;
        for (int i = 0; i < listaNombres.Count; i++) {
            if (listaNombres[i].text != Username) {
                if (listaNombres[i].text == "Username") {
                    listaNombres[i].text = Username;
                    break;
                }
            }
        }
    }
    public void Logout() {
        for (int i = 0; i < objectosUsuario.Count; i++) {
            objectosUsuario[i].SetActive(false);
        }
        for (int i = 0; i < objectosLogin.Count; i++) {
            objectosLogin[i].SetActive(true);
        }
    }
    public void ChangeScore() {
        StartCoroutine(NewScore());
    }
    IEnumerator NewScore() {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                UserData myData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
                if (myData.score == null) {
                    myData.score = "0";
                }
                myData.score = GameObject.Find("InputScore").GetComponent<TMP_InputField>().text;
                scoreText.text = "Your score is: " + myData.score;
                for (int i = 0; i < listaPuntajes.Count; i++) {
                    if (listaNombres[i].text == Username) {
                        listaPuntajes[i].text = myData.score;

                        valoresPuntajes[i] = int.Parse(listaPuntajes[i].text);
                    }
                }
                TMP_Text temp;
                int temp2;
                
                for (int i = 0; i < listaPuntajes.Count; i++) {
                    for (int j = 0; j < listaPuntajes.Count - 1; j++) {
                        if (valoresPuntajes[j] < valoresPuntajes[j + 1]) {
                            temp = listaPuntajes[j];
                            listaPuntajes[j] = listaPuntajes[j + 1];
                            listaPuntajes[j + 1] = temp;

                            temp = listaNombres[j];
                            listaNombres[j] = listaNombres[j + 1];
                            listaNombres[j + 1] = temp;

                            temp2 = valoresPuntajes[j];
                            valoresPuntajes[j] = valoresPuntajes[j + 1];
                            valoresPuntajes[j + 1] = temp2;
                        }
                    }
                }
                Debug.Log(myData.score);
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
}

[System.Serializable]
public class User {
    public string _id;
    public string username;
    public string password;
    public UserData data;

    public User() {
    }
    public User(string username, string password) {
        this.username = username;
        this.password = password;
    }
}
public class UserData {
    public string score;
}

public class AuthJsonData {
    public User usuario;
    public string token;
}