using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;

[System.Serializable]
public class BookList
{
    public List<Book> books = new List<Book>(); //List for Playerprefs
}

public class LibraryManager : MonoBehaviour
{
    [Header("BookList")]
    [SerializeField] private List<Book> bookList = new List<Book>();
    [SerializeField] private List<Book> borrowList = new List<Book>();
    public List<Book> outOfDateList = new List<Book>();

    [Space(10)]

    [Header("Input")]
    public TMP_InputField titleInput;
    public TMP_InputField authorInput;
    public TMP_InputField copyCountInput;
    public TMP_InputField ISBNInput;

    [Space(10)]

    [Header("Object")]
    public Button addBook;
    public GameObject errorScreen;
    public GameObject contentBookList;
    public GameObject contentBorrowList;
    public GameObject contentOutOfDateList;
    public GameObject bookPrefab;
    public GameObject bookBorrowPrefab;

    public static LibraryManager instance;

    [Header("Time")]
    [SerializeField] private int loanPeriod;
    [SerializeField] private int waitSeconds;
    private const string TimerKey = "BookTimer";
    DateController dateController;

    void Awake()
    {
        if (instance == null)
            instance = this;

        else
            Destroy(gameObject);

        dateController = GetComponent<DateController>();
    }

    public void Start()
    {
        errorScreen.SetActive(false);
        LoadBookList();
        LoadBookTimer();

        BookList(bookPrefab, contentBookList, bookList);
        BookList(bookBorrowPrefab, contentBorrowList, borrowList);
        BookList(bookBorrowPrefab, contentOutOfDateList, outOfDateList);
    }

    public void AddBook() //AddButton
    {
        if (titleInput.text != string.Empty && authorInput.text != string.Empty &&
            copyCountInput.text != string.Empty && ISBNInput.text != string.Empty &&
            BookNumberCheck(int.Parse(ISBNInput.text)))
        {
            //If the inputs are not empty, we add a new book to the list.
            Book newBook = new Book
            {
                title = titleInput.text,
                author = authorInput.text,
                copyCount = int.Parse(copyCountInput.text),
                ISBN = int.Parse(ISBNInput.text),
                timer = loanPeriod,
            };

            bookList.Add(newBook);
            BookList(bookPrefab, contentBookList, bookList);
            SaveBookList();

            //Then we refresh the entries
            titleInput.text = string.Empty;
            authorInput.text = string.Empty;
            copyCountInput.text = string.Empty;
            ISBNInput.text = string.Empty;
        }
        else
        {
            //If a book with the same barcode is entered, it gives an error.
            errorScreen.SetActive(true);
        }
    }

    public void BookList(GameObject obj, GameObject parent, List<Book> list) //New book object
    {
        foreach (Transform child in parent.transform) //list cleaning
        {
            Destroy(child.gameObject);
        }

        foreach (Book book in list) //We create a new object and assign its properties
        {
            GameObject bookClone = Instantiate(obj, parent.transform);
            BookProperties bookProperties = bookClone.GetComponent<BookProperties>();

            if (bookProperties != null)
            {
                bookProperties.titleText.text = book.title;
                bookProperties.authorText.text = book.author;
                bookProperties.countText.text = book.copyCount.ToString();
                bookProperties.ISBNText.text = book.ISBN.ToString();
                bookProperties.timer = book.timer;

                if (list == borrowList) //Time control of borrowed books
                    StartCoroutine(UpdateTime(bookProperties, book,false));
            }
            else
                Debug.LogError("bookProperties Null");
        }
    }

    public void BorrowBook(GameObject obj) //TakeButton
    {
        if (obj != null)
        {
            BookProperties bookProperties = obj.GetComponent<BookProperties>();

            if (bookProperties != null)
            {
                if (int.TryParse(bookProperties.countText.text, out int countValue))
                {
                    int childIndex;
                    childIndex = obj.transform.GetSiblingIndex();

                    //Remove the object if it is not left after borrowing it
                    if (countValue <= 1)
                    {
                        Destroy(obj);

                        bookList.RemoveAt(childIndex);
                    }
                    else
                    {
                        countValue--;
                        bookList[childIndex].copyCount--;

                        bookProperties.countText.text = countValue.ToString();
                    }

                    //We add the book we bought to our borrow list.
                    Book newBook = new()
                    {
                        title = bookProperties.titleText.text,
                        author = bookProperties.authorText.text,
                        copyCount = int.Parse(bookProperties.countText.text),
                        ISBN = int.Parse(bookProperties.ISBNText.text),
                    };

                    borrowList.Add(newBook);

                    GameObject bookClone = Instantiate(bookBorrowPrefab, contentBorrowList.transform);
                    BookProperties bookPro = bookClone.GetComponent<BookProperties>();

                    bookPro.titleText.text = newBook.title;
                    bookPro.authorText.text = newBook.author;
                    bookPro.countText.text = newBook.copyCount.ToString();
                    bookPro.ISBNText.text = newBook.ISBN.ToString();
                    newBook.timer = loanPeriod;
                    bookPro.timer = newBook.timer;


                    SaveBookTimer(bookPro.timer);
                    StartCoroutine(UpdateTime(bookPro, newBook,true));
                    SaveBookList();
                }
            }
        }
    }

    public void OutOfDate(GameObject obj) //BookProperties
    {
        if (obj != null)
        {
            int childIndex = obj.transform.GetSiblingIndex();

            if (childIndex < borrowList.Count)
            {
                Book dateBook = borrowList[childIndex];

                if(dateBook.timer <= 0)
                {
                    //List order of outdated books
                    borrowList.RemoveAt(childIndex);
                    outOfDateList.Add(dateBook);

                    //We changed its appearance and used the same object
                    obj.transform.SetParent(contentOutOfDateList.transform);
                    SaveBookList();
                }
            }
        }
    }

    public void GiveBack(GameObject obj) //GiveButton
    {
        bool bookFound = false;
        int i = 0;
        int childIndex;
        childIndex = obj.transform.GetSiblingIndex();

        if (obj != null)
        {
            BookProperties bookProperties = obj.transform.GetComponent<BookProperties>();
            bookProperties.timer = loanPeriod;

            foreach (Book book in bookList)
            {
                //If the numbers match, increase the number in the main list
                if (book.ISBN == int.Parse(bookProperties.ISBNText.text))
                {
                    book.copyCount++;
                    int countVal = book.copyCount;
                    contentBookList.transform.GetChild(i).GetComponent<BookProperties>().countText.text
                        = countVal.ToString();

                    i = 0;
                    bookFound = true;
                    break;
                }
                i++;
            }
            //If no book is found in the list, create a new book
            if (!bookFound)
            {
                Book newBook = new()
                {
                    title = bookProperties.titleText.text,
                    author = bookProperties.authorText.text,
                    copyCount = 1,
                    ISBN = int.Parse(bookProperties.ISBNText.text),
                    timer = loanPeriod,
                };

                bookList.Add(newBook);
                BookList(bookPrefab, contentBookList, bookList);
            }
            Destroy(obj);
            borrowList.RemoveAt(childIndex);
            SaveBookList();
        }
    }

    bool BookNumberCheck(int number) //ISBN
    {
        foreach (Book book in bookList)
        {
            if (book.ISBN == number)
                return false;
        }
        return true;
    }

    #region DATETIME

    private IEnumerator UpdateTime(BookProperties bookProperties, Book bookForTime , bool newBook)
    {
        if (!newBook)
        {
            //Time spent with game closed
            bookForTime.timer -= dateController.minutesPassed;
            bookProperties.timer = bookForTime.timer;
            SaveBookTimer(bookForTime.timer);
            SaveBookList();
        }

        while (true)
        {
            yield return new WaitForSeconds(waitSeconds);
            
            bookForTime.timer--;
            
            bookProperties.timer = bookForTime.timer;
            
            SaveBookTimer(bookForTime.timer);
            SaveBookList();
        }
    }

    #endregion

    #region JSON

    private void SaveBookTimer(float timerValue)
    {
        PlayerPrefs.SetFloat(TimerKey, timerValue);
        PlayerPrefs.Save();
    }

    private float LoadBookTimer()
    {
        return PlayerPrefs.GetFloat(TimerKey, 4f);
    }

    private void SaveBookList()
    {
        string bookListJson = JsonUtility.ToJson(new BookList { books = bookList });
        PlayerPrefs.SetString("BookListKey", bookListJson);

        string borrowListJson = JsonUtility.ToJson(new BookList { books = borrowList });
        PlayerPrefs.SetString("BorrowBookListKey", borrowListJson);


        string outOfDateListJson = JsonUtility.ToJson(new BookList { books = outOfDateList });
        PlayerPrefs.SetString("OutOfDateListKey", outOfDateListJson);
        PlayerPrefs.Save();
    }

    private void LoadBookList()
    {
        if (PlayerPrefs.HasKey("BookListKey"))
        {
            string bookListJson = PlayerPrefs.GetString("BookListKey");
            BookList loadedBookList = JsonUtility.FromJson<BookList>(bookListJson);

            if (loadedBookList != null)
            {
                bookList = loadedBookList.books;
            }
        }

        if (PlayerPrefs.HasKey("BorrowBookListKey"))
        {
            string borrowListJson = PlayerPrefs.GetString("BorrowBookListKey");
            BookList loadedBorrowBookList = JsonUtility.FromJson<BookList>(borrowListJson);

            if (loadedBorrowBookList != null)
            {
                borrowList = loadedBorrowBookList.books;
            }
        }
        if (PlayerPrefs.HasKey("OutOfDateListKey"))
        {
            string outOfDateListJson = PlayerPrefs.GetString("OutOfDateListKey");
            BookList loadedOutOfDateList = JsonUtility.FromJson<BookList>(outOfDateListJson);

            if (loadedOutOfDateList != null)
            {
                outOfDateList = loadedOutOfDateList.books;
            }
        }
    }
    #endregion
}
