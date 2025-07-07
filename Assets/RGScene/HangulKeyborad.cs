using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HangulKeyborad : MonoBehaviour
{

    public TMP_InputField korField;
    ////VirtualKeyboard
    public GameObject wndKeyBoard;

    //public GameObject[] NumberKey;
    public GameObject[] LangKey;
    public GameObject[] LangKey2; //�����е���

    public GameObject goShiftKey;
    public GameObject goSymbolkey;

    [SerializeField] bool isEngOn = true;
    private bool isSymbolAct = false;
    private bool isShift = false;

    private int caretPosition = 0;//����Ŀ��������

    string[] EngKey = { "q", "w", "e", "r", "t", "y",
        "u","i", "o","p", "a","s", "d","f", "g", "h", "j", "k", "l", "z", "x","c", "v","b", "n","m"};

    string[] EngShiftKey = { "Q", "W", "E", "R", "T", "Y",
        "U", "I", "O","P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X","C", "V", "B", "N","M"};

    string[] korKey = { "��", "��", "��", "��", "��", "��",
        "��","��", "��","��", "��","��", "��","��", "��", "��", "��", "��", "��", "��", "��","��", "��","��", "��","��"};

    string[] korShiftKey = { "��", "��", "��", "��", "��", "��",
        "��","��", "��","��", "��","��", "��","��", "��", "��", "��", "��", "��", "��", "��","��", "��","��", "��","��"};

    string[] NumKey = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };

    string[] SymbolKey = { "\'", "~", "-", "_", "=", "+",
        "\\", "|", "[","]", ";", ":", "'", "\"", "{", "}", "<", ">", "?", ",", ".","/","��", "��", "��", "��"};

    string[] SymbolKey2 = { "!", "@", "#", "$", "%", "^", "&", "*", "(", ")" };

    //�ѱ�Ű����
    char[] chosung_index = { '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' }; //19��
    char[] joongsung_index = { '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' }; //22��
    char[] jongsung_index = {' ' , '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', //28��
          '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' };

    char[] Jcombo_index = { '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' }; //11�� 
    string[] Jcombo = { "����", "����", "����", "����", "����", "����", "����", "����", "����", "����", "����" };
    char[] Mcombo_index = { '��', '��', '��', '��', '��', '��', '��' }; //7�� 
    string[] Mcombo = { "�Ǥ�", "�Ǥ�", "�Ǥ�", "�̤�", "�̤�", "�̤�", "�Ѥ�" };

    char chKorInput = ' '; //���� ����
                           //kor UniCode = (start * 21 + mid )*28 + End + 0xAC00

    
    //InputKey
    public void OnClicked(Text text)
    {
        OnClickedOnKor(korField, text);
    }

    void OnClickedOnKor(TMP_InputField inputfiled, Text text)
    {
        //�տ� ���ڰ� ���ų� �ձ��ڰ� �ѱ��� �ƴѰ�� -> �׳� �ٷ� ���
        if (inputfiled.text.Length == 0 || !isKorean(inputfiled.text[inputfiled.text.Length - 1]) || !isKorean(text.text[0]))
        {
            string newText = text.text;
            inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, newText);
        }
        else//�տ� ���ڰ� �ִ°��
        {
            chKorInput = text.text[0];
            //�������� �������� ����
            char JM = isJaOrMo(chKorInput);
            char lasttext = inputfiled.text[inputfiled.text.Length - 1];

            //1. ������ ������ �ʼ�,�߼�,������ �ε��� ���ϱ�
            char chosung = ' ';
            char joongsung = ' ';
            char jongsung = ' ';

            int jong_idx = -1;
            int joong_idx = -1;
            int cho_idx = -1;

            //������ ���ڰ� �ϳ��� ������ �ִ� ��� 
            if (isJaOrMo(lasttext) == 'J')
            {
                chosung = lasttext;
                joongsung = ' ';
                jongsung = ' ';

                //������ ���ڰ� �ϳ��� ������ �ִ� ��� 
            }
            else if (isJaOrMo(lasttext) == 'M')
            {
                chosung = ' ';
                joongsung = lasttext;
                jongsung = ' ';

            }
            else
            {//������ ���ڰ� �ϳ��� �����̳� ������ �ƴѰ�� 

                //������ ���ڿ��� AC00�� ����
                var lastchar_uni_cal = lasttext - 44032;

                //������ ������ �ʼ�, �߼�, ������ �ε��� ���ϱ� 
                //�ѱ�������ġ = ((�ʼ�index * 21) + �߼�index) * 28 + ����index
                jong_idx = lastchar_uni_cal % 28;
                joong_idx = Mathf.FloorToInt(lastchar_uni_cal / 28) % 21;
                cho_idx = Mathf.FloorToInt((Mathf.FloorToInt(lastchar_uni_cal / 28)) / 21);

                //������ ������ �ʼ�, �߼�, ���� ���ϱ� 
                chosung = chosung_index[cho_idx];
                joongsung = joongsung_index[joong_idx];
                jongsung = jongsung_index[jong_idx];
            }

            string str_uni = string.Empty;
            int key_idx = 0;

            //���� ������
            if (lasttext == chosung && JM == 'J')//1 . �տ� ���� + ����
            {
                string newja = string.Empty;
                newja = newja + chosung;
                newja = newja + chKorInput;

                key_idx = getIndexinArray<string>(Jcombo, newja); // -1 �̸� �޺��� ���°��

                if (key_idx != -1)
                {
                    string newText = string.Empty;
                    newText += Jcombo_index[key_idx];
                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, newText);
                }
                else//�޺��ƴѰ�� �׳� �߰�
                {
                    string newText = text.text;
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, newText);
                }

            }
            else if (lasttext == chosung && JM == 'M') // 2. �տ� ������ �ִ� ��� + ����
            {

                for (int i = 0; i < joongsung_index.Length; ++i)
                {
                    if (joongsung_index[i] == chKorInput)
                    {
                        key_idx = i;
                        break;
                    }
                }

                int indexCombo = -1;
                //�տ� ������ �޺��� ��� ex) ��
                indexCombo = getIndexinArray<char>(Jcombo_index, lasttext);

                if (indexCombo != -1)
                {
                    //�޺������� �� �� �� �� ��  ex) �� + �� = ��
                    if (lasttext == '��' || lasttext == '��' || lasttext == '��' || lasttext == '��' || lasttext == '��')
                    {
                        int Cho_idx = -1;
                        for (int i = 0; i < chosung_index.Length; i++)
                        {
                            if (lasttext == chosung_index[i])
                            {
                                Cho_idx = i;
                                break;
                            }
                        }

                        char newJM = (char)(((Cho_idx * 21) + key_idx) * 28 + 44032);
                        str_uni += newJM;

                        inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                        inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);

                    }
                    else //ex) �� + �� = ����
                    {
                        char ja1 = Jcombo[indexCombo][0];
                        char ja2 = Jcombo[indexCombo][1];

                        int Cho_idx = -1;
                        for (int i = 0; i < chosung_index.Length; i++)
                        {
                            if (ja2 == chosung_index[i])
                            {
                                Cho_idx = i;
                                break;
                            }
                        }

                        char newJM = (char)(((Cho_idx * 21) + key_idx) * 28 + 44032);

                        str_uni += ja1;
                        str_uni += newJM;

                        inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                        inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                    }
                }
                else //  no combo  ex_ �� + �� = ��
                {
                    int Cho_idx = -1;
                    for (int i = 0; i < chosung_index.Length; i++)
                    {
                        if (lasttext == chosung_index[i])
                        {
                            Cho_idx = i;
                            break;
                        }
                    }

                    char newJM = (char)(((Cho_idx * 21) + key_idx) * 28 + 44032);
                    str_uni += newJM;

                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }

            }
            else if (lasttext == joongsung && JM == 'J') //3, �տ� ������ �ִ°�� + ����
            {

                str_uni += chKorInput;
                inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
            }
            else if (lasttext == joongsung && JM == 'M')// 4, �տ� ������ �ִ°�� + ����
            {
                string newMo = string.Empty;
                newMo += lasttext;
                newMo += chKorInput;

                key_idx = getIndexinArray<string>(Mcombo, newMo);

                //combo
                if (key_idx != -1)
                {
                    str_uni += Mcombo_index[key_idx];
                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
                else // �׳� �ڿ� �߰�
                {
                    str_uni += chKorInput;
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }

            }
            else if (jongsung != ' ' && JM == 'J')//5.���� ���ڰ� ������ �ִ� ��� + ����
            {
                string newJong = string.Empty;
                newJong += jongsung;
                newJong += chKorInput;

                key_idx = getIndexinArray<string>(Jcombo, newJong);

                //Combo
                if (key_idx != -1)
                {
                    cho_idx = getIndexinArray<char>(chosung_index, chosung);
                    joong_idx = getIndexinArray<char>(joongsung_index, joongsung);
                    char newJongsung = Jcombo_index[key_idx];
                    jong_idx = getIndexinArray<char>(jongsung_index, newJongsung);


                    char newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + jong_idx + 44032);
                    str_uni += newCh;
                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
                else
                {
                    str_uni += chKorInput;
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }

            }
            else if (jongsung != ' ' && JM == 'M')//6.���� ���ڰ� ������ �ִ� ��� + ����
            {
                key_idx = getIndexinArray<char>(Jcombo_index, jongsung);

                //������ Combo ex) �� + �� = ����
                if (key_idx != -1)
                {
                    string newjong = Jcombo[key_idx];  //����
                    char newjong1 = newjong[0]; //��
                    char newjong2 = newjong[1]; //��

                    //��
                    cho_idx = getIndexinArray<char>(chosung_index, chosung);
                    joong_idx = getIndexinArray<char>(joongsung_index, joongsung);
                    jong_idx = getIndexinArray<char>(jongsung_index, newjong1);

                    char newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + jong_idx + 44032);
                    str_uni += newCh;
                    //��
                    cho_idx = getIndexinArray<char>(chosung_index, newjong2);
                    joong_idx = getIndexinArray<char>(joongsung_index, chKorInput);
                    newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
                    str_uni += newCh;

                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
                else //������ Combo NO  ex) �� + �� = ����
                {
                    //��
                    cho_idx = getIndexinArray<char>(chosung_index, chosung);
                    joong_idx = getIndexinArray<char>(joongsung_index, joongsung);

                    char newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
                    str_uni += newCh;

                    cho_idx = getIndexinArray<char>(chosung_index, jongsung);
                    joong_idx = getIndexinArray<char>(joongsung_index, chKorInput);
                    newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
                    str_uni += newCh;

                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
            }
            else if (jongsung == ' ' && JM == 'J')//7.���� ���ڰ� ������ ���� ��� + ����   ex) �� + �� = �� , �� + �� = ����
            {
                key_idx = getIndexinArray<char>(jongsung_index, chKorInput);

                //�Է��� ������ ��ħ�� �� �� �ִ� ���
                if (key_idx != -1)
                {
                    cho_idx = getIndexinArray<char>(chosung_index, chosung);
                    joong_idx = getIndexinArray<char>(joongsung_index, joongsung);
                    jong_idx = getIndexinArray<char>(jongsung_index, chKorInput);

                    char newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + jong_idx + 44032);
                    str_uni += newCh;
                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
                else//��ħ�� �� �� ���� ���
                {
                    str_uni += chKorInput;
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
            }
            else if (jongsung == ' ' && JM == 'M')//8.���� ���ڰ� ������ ���� ��� + ���� 
            {
                string newMo = string.Empty;
                newMo += joongsung;
                newMo += chKorInput;

                key_idx = getIndexinArray<string>(Mcombo, newMo);

                //�޺��ΰ�� ex. �� + �� = ��
                if (key_idx != -1)
                {
                    char mo = Mcombo_index[key_idx];

                    joong_idx = getIndexinArray<char>(joongsung_index, mo);

                    char newCh = (char)(((cho_idx * 21) + joong_idx) * 28 + 44032);
                    str_uni += newCh;
                    inputfiled.text = inputfiled.text.Remove(inputfiled.text.Length - 1);
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
                else//�������� ����(�߼�) + ģ����(����) �޺� �ƴѰ�� ex.�� + �� = ����
                {
                    str_uni += chKorInput;
                    inputfiled.text = inputfiled.text.Insert(inputfiled.text.Length, str_uni);
                }
            }
        }
    }

    char isJaOrMo(char uni)
    {
        if (uni >= 12593 && uni <= 12622)
        {
            return 'J';
        }
        else if (uni >= 12623 && uni <= 12643)
        {
            return 'M';
        }
        else
        {
            return ' ';
        }
    }

    bool isKorean(char uni)
    {
        if ((uni >= 12593 && uni <= 12622) || (uni >= 12623 && uni <= 12643) || uni >= 0xAC00 && uni <= 0xD7AF)
            return true;
        else
            return false;
    }

    public static int getIndexinArray<T>(T[] arr, T value)
    {
        int index = -1;

        for (int i = 0; i < arr.Length; ++i)
        {
            if (arr[i].Equals(value))
            {
                index = i;
                break;
            }
        }

        return index;
    }

    //InputKey_Backspace
    public void OnBackspaceClicked()
    {
        if (korField.text.Length <= 0) return;
        korField.text = korField.text.Substring(0, korField.text.Length - 1);
        caretPosition--;
    }

    //ShiftKey Change
    public void OnShiftClicked()
    {
        if (isSymbolAct)
            OnSymbolClicked();

        isShift = !isShift;

        if (isShift)
        {
            goShiftKey.transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 255);
        }
        else
        {
            goShiftKey.transform.GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255);
        }

        if (isEngOn)
        {
            if (isShift)
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = EngShiftKey[i];
                }
            }
            else
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = EngKey[i];
                }
            }
        }
        else
        {
            if (isShift)
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = korShiftKey[i];
                }
            }
            else
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = korKey[i];
                }
            }
        }
    }

    //korean/EnglishKey Change
    public void OnKorEngClicked()
    {
        if (isSymbolAct)
            OnSymbolClicked();

        isEngOn = !isEngOn;

        if (isEngOn)
        {
            if (isShift)
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = EngShiftKey[i];
                    //LangKey[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = korShiftKey[i];
                }
            }
            else
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = EngKey[i];
                    //LangKey[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = korKey[i];
                }
            }
        }
        else
        {
            if (isShift)
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = korShiftKey[i];
                    //LangKey[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = EngShiftKey[i];
                }
            }
            else
            {
                for (int i = 0; i < LangKey.Length; ++i)
                {
                    LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = korKey[i];
                    //LangKey[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = EngKey[i];
                }
            }
        }

    }

    //SymbolKey Change
    public void OnSymbolClicked()
    {
        isSymbolAct = !isSymbolAct;

        //�ε������ Ȯ���غ� ��//
        isShift = false;
        goShiftKey.transform.GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255);

        if (isSymbolAct)
        {
            for (int i = 0; i < SymbolKey.Length; ++i)
            {
                LangKey[i].GetComponentInChildren<Text>().text = SymbolKey[i];
                LangKey[i].transform.GetChild(1).gameObject.SetActive(false);
                LangKey[i].transform.GetChild(0).GetComponent<LayoutElement>().preferredWidth = 88;
                LangKey[i].transform.GetChild(0).GetComponent<LayoutElement>().preferredHeight = 88;
            }
            for (int i = 0; i < SymbolKey2.Length; ++i)
            {
                LangKey2[i].GetComponentInChildren<Text>().text = SymbolKey2[i];
            }
        }
        else
        {
            for (int i = 0; i < EngShiftKey.Length; ++i)
            {
                LangKey[i].GetComponentInChildren<Text>().text = EngShiftKey[i];
                LangKey[i].transform.GetChild(1).gameObject.SetActive(true);
                LangKey[i].transform.GetChild(0).GetComponent<LayoutElement>().preferredWidth = 44;
                LangKey[i].transform.GetChild(0).GetComponent<LayoutElement>().preferredHeight = 44;
            }
            for (int i = 0; i < NumKey.Length; ++i)
            {
                LangKey2[i].GetComponentInChildren<Text>().text = NumKey[i];
            }
        }
    }

    void Start()
    {
        //�빮�ڷ� �Ǿ� �ִ°� �ҹ��ڷ� ��ȯ ���ذ�
        foreach (GameObject key in LangKey)
        {
            key.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text =
                key.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text.ToLower();
        }

        if (isEngOn)
        {
            for (int i = 0; i < LangKey.Length; ++i)
            {
                LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = EngKey[i];
            }
        }
        else
        {
            for (int i = 0; i < LangKey.Length; ++i)
            {
                LangKey[i].transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = korKey[i];
            }
        }
    }

}
