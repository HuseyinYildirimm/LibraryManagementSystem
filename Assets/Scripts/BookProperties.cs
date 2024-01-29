using UnityEngine;
using TMPro;
using System;
using System.Collections;

public enum BookType
{
    book,
    borrowBook,
    outOfDateBook
}

public class BookProperties : MonoBehaviour
{
    public BookType bookType;

    [Space(10)]

    [SerializeField] private GameObject giveButton;

    [Space(10)]

    [Header("TEXT")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI authorText;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI ISBNText;
    public TextMeshProUGUI timeText;

    LibraryManager libraryManager;

    [Space(10)]

    [Header("TIME")]
    [HideInInspector] public float timer;

    public void Awake()
    {
        libraryManager = LibraryManager.instance;
    }

    public void Update()
    {
        if (bookType == BookType.borrowBook)
        {
            if (timer <= 0)
            {
                if (giveButton != null)
                    giveButton.SetActive(false);

                libraryManager.OutOfDate(this.gameObject);
                bookType = BookType.outOfDateBook;
            }
        }
        if (timer < 0) timer = 0;

        if(bookType != BookType.book)
        {
            timeText.text = timer + " Day";
        }
    }

    public void TakeButton()
    {
        if (libraryManager != null)
        {
            libraryManager.BorrowBook(gameObject);
        }
    }

    public void GiveButton()
    {
        if (libraryManager != null)
        {
            libraryManager.GiveBack(gameObject);
        }
    }
}


