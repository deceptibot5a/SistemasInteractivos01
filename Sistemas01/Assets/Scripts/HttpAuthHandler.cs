using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HttpAuthHandler : MonoBehaviour
{
    [SerializeField]
    private string ServerApiURL;

    public string Token { get; set; }
    public string Username { get; set; }

    void Start() {
        List<User> lista = new List<User>();
        var listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();

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
        StartCoroutine(Registro(postData));
    }

    public void Ingresar() {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<TMP_InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<TMP_InputField>().text;
        string postData = JsonUtility.ToJson(user);
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
                //Proceso de autenticacion
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
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator GetPerfil() {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/"+Username);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");
                //Cambiar de escena
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
public class User
{
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

public class AuthJsonData
{
    public User usuario;
    public string token;
}