using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

public class HttpAuthHandler : MonoBehaviour
{
    [SerializeField] private string ServerApiURL;
    [SerializeField] GameObject objetoLogin, objetoIndex;
    [SerializeField] TMP_Text userText, userError;
    [SerializeField] TMP_Text[] listaNombres, listaPuntajes;

    public string Token { get; set; }
    public string Username { get; set; }
    private string token;

    void Start() {
        List<User> lista = new List<User>();
        List<User> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();

        if (string.IsNullOrEmpty(Token)) {
            objetoIndex.SetActive(false);
            objetoLogin.SetActive(true);
            Debug.Log("No hay token");
        } else {
            objetoLogin.SetActive(false);
            objetoIndex.SetActive(true);
            token = Token;
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
    public void Score() {
        User user = new User();
        user.username = Username;
        if (int.TryParse(GameObject.Find("InputScore").GetComponent<TMP_InputField>().text, out _)) {
            user.data.score = int.Parse(GameObject.Find("InputScore").GetComponent<TMP_InputField>().text);
        }
        string postData = JsonUtility.ToJson(user);
        Debug.Log(postData);
        StartCoroutine(NewScore(postData));
    }
    public void LogOut() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                userError.text = "Error : El usuario ya existe  ";
                StartCoroutine(ErrorWait());
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
                objetoLogin.SetActive(false);
                objetoIndex.SetActive(true);
                StartCoroutine(ChangeScore());
                userText.text = "Usuario :" + jsonData.usuario.username;
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
                userError.text = "Usuario inexistente o contraseña incorrecta";
                StartCoroutine(ErrorWait());
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
                userText.text = "Usuario :" + jsonData.usuario.username;
                StartCoroutine(ChangeScore());
            } else {
                objetoIndex.SetActive(false);
                objetoLogin.SetActive(true);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                userError.text = "Error : El usuario anterior a cerrado seccion ";
                StartCoroutine(ErrorWait());
                Debug.Log(mensaje);
            }
        }
    }
    IEnumerator ErrorWait() {
        yield return new WaitForSeconds(5f);
        userError.text = "";
    }
    IEnumerator ChangeScore() {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios");
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.isNetworkError) {
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200) {
                userlist jsonList = JsonUtility.FromJson<userlist>(www.downloadHandler.text);
                List<User> lista2 = jsonList.usuarios;
                List<User> listaOrdenada2 = lista2.OrderByDescending(u => u.data.score).ToList<User>();
                int userPos = 0;
                foreach (User person in listaOrdenada2) {
                    if (userPos >= 4) {
                    } else {
                        string _username = userPos + 1 + " - " + person.username + " - " + person.data.score;
                        listaNombres[userPos].text = _username;
                        userPos++;
                    }
                }
            } else {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator NewScore(string postData) {
        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios/", postData);
        www.method = "PATCH";
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.isNetworkError) {
            objetoIndex.SetActive(false);
            objetoLogin.SetActive(true);
            Debug.Log("NETWORK ERROR :" + www.error);
        } else {
            Debug.Log(www.downloadHandler.text);
            if (www.responseCode == 200) {
                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);
                StartCoroutine(ChangeScore());
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
        data = new UserData();
    }
    public User(string username, string password) {
        this.username = username;
        this.password = password;
        data = new UserData();
    }
}
[System.Serializable]
public class UserData {
    public int score;
}

public class AuthJsonData {
    public User usuario;
    public UserData data;
    public string token;
}
[System.Serializable]
public class userlist {
    public List<User> usuarios;
}