using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewBookList", menuName ="BookList")]
public class BookScriptableObject : ScriptableObject
{
    public List<Book> books = new List<Book>();
}
