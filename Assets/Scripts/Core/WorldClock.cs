using Action = System.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldClock : MonoBehaviour
{
    public static WorldClock Instance { get; private set; }

    [SerializeField]
    private int _minute = 21;
    [SerializeField]
    private int _hour = 7;
    [SerializeField]
    private int _day = 5;
    [SerializeField]
    private Month _month = Month.December;
    [SerializeField]
    private int _year = 2161;

    [SerializeField]
    private int _dayTicks = 0;//The number of days that have passed since the game started.

    private readonly WaitForSeconds _normalTime = new WaitForSeconds(1);

    #region Actions

    public Action morningStarted;
    public Action nightStarted;

    public Action newDay;
    public Action newMonth;
    public Action newYear;

    #endregion
    #region Properties

    public bool IsLeapYear
    {
        get
        {
            var isLeapYear = false;
            if (_year % 4 == 0)
            {
                if (_year % 100 == 0)
                {
                    if (_year % 400 == 0)
                    {
                        isLeapYear = true;
                    }
                }
                else
                {
                    isLeapYear = true;
                }
            }
            return isLeapYear;
        }
    }

    public bool IsNight => _hour <= MORNING || _hour >= EVENING;

    #endregion
    #region Constants

    private const int MORNING = 6;
    private const int EVENING = 18;
    
    private const int HOUR = 60;
    private const int DAY = 24;
    private const int LONGMONTH = 31;
    private const int MEDMONTH = 30;
    private const int FEBLEAPMONTH = 29;
    private const int FEBMONTH = 28;
    private const int YEAR = 12;
    #endregion
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //StartCoroutine(RunClock());
    }

    private IEnumerator RunClock()
    {
        while (enabled)
        {
            yield return _normalTime;
            _minute++;
            if (_minute < HOUR)
            {
                continue;
            }

            _minute = 0;
            _hour++;
            
            if (_hour < DAY)
            {
                continue;
            }
            _hour = 0;
            _day++;
            _dayTicks++;
            
            switch (_month)
            {
                case Month.January:
                case Month.March:
                case Month.May:
                case Month.July:
                case Month.August:
                case Month.October:
                case Month.December:
                    if (_day <= LONGMONTH)
                    {
                        newDay?.Invoke();
                        continue;
                    }
                    break;
                case Month.February:
                    if (IsLeapYear)
                    {
                        if (_day <= FEBLEAPMONTH)
                        {
                            newDay?.Invoke();
                            continue;
                        }
                    }
                    else
                    {
                        if (_day <= FEBMONTH)
                        {
                            newDay?.Invoke();
                            continue;
                        }
                    }
                    break;
                case Month.April:
                case Month.June:
                case Month.September:
                case Month.November:
                    if (_day <= MEDMONTH)
                    {
                        newDay?.Invoke();
                        continue;
                    }
                    break;
                default:
                    Debug.LogError("The shit is happening? What month is it? Stopping the Clock!");
                    yield break;
            }

            _day = 1;
            if ((int) _month < YEAR)
            {
                _month++;
                newDay?.Invoke();
                newMonth?.Invoke();
                continue;
            }
            else
            {
                _month = Month.January;
                _year++;
            }
            
            newDay?.Invoke();
            newMonth?.Invoke();
            newYear?.Invoke();
        }
    }

    private enum Month
    {
        January = 1,
        February = 2,
        March = 3,
        April = 4,
        May = 5,
        June = 6,
        July = 7,
        August = 8,
        September = 9,
        October = 10,
        November = 11,
        December = 12
    }
}
