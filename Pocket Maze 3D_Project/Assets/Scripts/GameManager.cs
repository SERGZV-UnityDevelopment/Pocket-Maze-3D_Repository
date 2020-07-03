using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// -При игре на телефоне если съесть первую вишню или первое замедление игра замирает возможно и на других уровнях глючат вишни и заморозка. Разобраться почему фризы и убрать их. Предположительно из за того что в первый раз материалы
// .Загружаються на сцену или в оперативную память компьютера с жёсткого диска - Если это так то решить проблему легко, нужно лишь сразу загрузить материалы на сцену. Если они уже загруженны значит проблема из за смены материала
// -На планшете игра выглядит ступенчато

// Исправления:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

// Факты об игре:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
// Скорость призраков для 10 уровня (Охотник - 10, Сидящий в засаде - 8, Стеснительный - 6, Домосед - 5) и пекмен 30
// Бонусы уровней -1) Ниего, 2) Вишня, 3) Ничего, 4) Ворота, 5) Ничего, 6) Замедление + Вишня, 7) Ничего, 8) Замедление + Заморозка, 9) Ничего, 10) Замедление + Заморозка + вишня.
// Бонусы уровней -1)Ничего  2)Вишня  3)2Вишни  4)Ворота+замедление  5)2Замедления  6)2Замедления+вишня  7)3Замедления  8)Замедление+Заморозка  9)2Заморозки+Замедление  10)Замедление+вишня+2Заморозки    


public class GameManager : MonoBehaviour 
{
	public enum ActModOfG : byte {Pursuit, RunUp, Slow, Freezing, Fright};	// Объект перечисления Общих (Активных) режимов для всех приведения на данный момент
	public ActModOfG ActiveModeOfGhosts;												// Активный на данный момент для всех приведений режим
	public delegate void NormalDelegate();												// Делегат для событий
	public static event NormalDelegate StartGame;									// Событие старта игры
	public static event NormalDelegate CherryWasTaken;								// Вишня была взята
	public static event NormalDelegate DecelerationWasTaken;						// "Замедление было взято"
	public static event NormalDelegate FreezingWasTaken;							// "Заморозка была взята"
	public static event NormalDelegate PacManNotDanger;							// Это событие вызываеться когда пекмен больше не опасен
	public static event NormalDelegate GhostReturnNormalSpeed;					// Событие "Призраки возвращают нормальную скорость"
	public static event NormalDelegate KillPacMan;									// Событие смерти пемена
	public static event NormalDelegate AllDotsAreCollected;						// Событие "Все точки собранны"
	public static event NormalDelegate StartLevel;									// Событие старта уровня
	public static event NormalDelegate NewGame;										// Событие новой игры (Можно выбрать после проигрыша или прохождения игры)
	public static event NormalDelegate ToPursuit;									// Событие переключения приведений в режим преследования
	public static event NormalDelegate ToRunUp;										// Событие переключения приведений в режим разбегания
	public static event NormalDelegate Pause;											// Это событие ставит игру на паузу
	public static event NormalDelegate Play;											// Это событие снимает игру с паузы
	public static event NormalDelegate EndGame;										// Событие конца игры, вызываеться когда пройден последний уровень
	public static event NormalDelegate ScreenNowPortrait;							// Событие "Экран теперь в режиме Portrait"
	public static event NormalDelegate ScreenNowLandscape;						// Событие "Экран теперь в режиме Landscape"
	public PacManMove PM;																	// Переменная для объекта пекмена скрипта PacManMove						
	public byte GameLevel;																	// Текущий уровень игры
	public bool PlayGame = false;             										// Режим игры (Игра или пауза)
	public short DotsLeft = 380;															// Количество дотов которые осталось собрать
	public short DotsEaten;																	// Количесвто съеденных дотов
	public int GamePoints;																	// Эта переменная содержит в себе количество очков которые заработал пекмен
	public sbyte PacmanLifes;																// Количество жизней пекмена
	public GameObject PacDots;																// Группа пекдотов
	public GameObject[] Ghosts = new GameObject[4];									// Призраки
	public Material RealMat;																// Действительный материал лабиринта
	public Material[] Mat = new Material[10];											// Массив материалов лабиринта
	public Material[] GhostsMats = new Material[4];									// Материалы для приведений (0-Материал глаз, 1-Материал страха, 2-Материал замедления, 3-Материал заморозки)
	public GameObject MainMenu;															// Переменная для главного канваса "MainMenu"
	public GameObject[] Gates = new GameObject[2];									// Массив из двух врат				
	const float PursuitTime = 25;															// Этот таймер отвечает за время нахождения приведения в режиме "Преследования"
	const float RunUpTime = 10;															// Этот таймер отвечает за время нахождения приведения в режиме "Разбегания"
	const float CherryTime = 20;															// Эта переменная указывает сколько времени действует вишня 20
	const float SlowTime = 20;																// Эта переменная указывает сколько времени действует режим замедления приведений
	const float FreezingTime = 10;														// Эта переменная указывает сколько времени действует режим заморозки приведений
	public GUInterface GUInt;																// Эта переменная для скрипта GUInterface		
	float _PacManDangerTimer = 0;															// Эта переменная указывает сколько секунд осталось до того как пекмен снова станет не опасным для приведений
	float PursuitRunUpTimer;																// Этот таймер отвечает за оставшееся время нахождения приведений в текущем из режимов (Преследование/Разбегание)
	float GhostSlowFreezeTimer = 0;														// Эта переменная указывает сколько секунд осталось до того как призрак выйдет из режима заморозки или замедления
	public bool PacManDanger = false;													// Переменная указывающая следует ли запускать таймер отсчитывающий время опасности пекмена и вызывать событие говорящее о его безопасности
	bool CallSlowGhostTimer = false;														// Переменная указывающая следует ли запускать таймер отсчитывающий время приведений проведённых в режиме замедления
	bool CallFreezingGhostTimer = false;												// Переменная указывающая следует ли запускать таймер отсчитывающий время приведений проведенных в режиме заморозки
	bool LaunchingAllDotsAreCollected = false;										// Эта переменная указывает был ло ли запущенно событие "AllDotsAreCollected"


	void OnEnable()
	{
		StartGame += MethodStartGame;										// Подписываем метод "MethodStartGame" на cобытие "StartGame" 
		CherryWasTaken += MethodCherryWasTaken;						// Подписываем метод "MethodCherryWasTaken" на событие "CherryWasTaken"
		DecelerationWasTaken += MethodDecelerationWasTaken;		// Подписываем метод "MethodDecelerationWasTaken" на событие "DecelerationWasTaken"
		FreezingWasTaken += MethodFreezingWasTaken;					// Подписываем метод "MethodFreezingWasTaken" на событие "FreezingWasTaken"
		PacManNotDanger += MethodPacManNotDanger;						// Подписываем метод "PacManNotDanger" на событие "MethodPacManNotDanger"
		KillPacMan += MethodKillPacMan;									// Подписываем метод "MethodKillPacMan" на событие "KillPacMan"
		AllDotsAreCollected += MethodAllDotsAreCollected;			// Подписываем метод "MethodAllDotsAreCollected" на cобытие "Все точки собранны" 
		Pause += MethodPause;												// Подписываем метод "MethodPause" на событие "Pause"
		Play += MethodPlay;													// Подписываем метод "MethodPlay" на событие "Play"
		NewGame += MethodNewGame;											// Подписываем метод "MethodNewGame" на событие "NewGame"
	} 


	void Start()
	{
		if(GameLevel == 1)RealMat.color = Mat[GameLevel].color;	// В начале ставим материал лабиринта как первый
		PursuitRunUpTimer = PursuitTime;									// Присваиваем переменной PursuitRunUpTimer значение переменной PursuitTime
	}


	void Update()
	{
		PacManDangerTimer();													// Вызываем таймер отсчитывающий время опасности пекмена
		GhostSlowTimer();														// Вызываем таймер отсчитывающий время нахождения призраков в режиме замедления
		GhostFreezingTimer();												// Вызываем таймер отсчитывающий время нахождения призраков в режиме заморозки
		MethodRunUpTimer();													// Вызываем метод отсчёта времени переключения между режимами погони и разбегания
		RoundEndCheck();														// Вызываем метод проверки конца раунда
	}


	void PacManDangerTimer()												// Метод отсчитывающий время опасности пекмена
	{
		if(PacManDanger == true)											// Если переменная "CallPacManTimer" равна правда
		{
			if(_PacManDangerTimer > 0)										// Если время в переменной PacManDangerTime больше ноля
				_PacManDangerTimer -= (1 * Time.deltaTime);			// Отнимаем по еденице каждую секунду
			else if(_PacManDangerTimer <= 0)								// Иначе если время в переменной PacManDangerTime меньше или равно нулю
			{
				PacManNotDanger();												// Вызываем событие PacManNotDanger говорящее что пекмен больше не опасен
				PursuitRunUpTimer = PursuitTime;								// И ставим значение переменной RunUpTimer на 15 секунд
				ToPursuit();														// Вызываем событие переключения призрака в режим "Преследования"
				ActiveModeOfGhosts = ActModOfG.Pursuit;					// Указываем общий для всех режим как "Погоня"
				PacManDanger = false;											// Указываем переменной "CallPacManTimer" что пока что больше не надо отсчитывать время и вызывать событие PacManNotDanger
			}
		}
	}


	void GhostSlowTimer()													// Таймер времени которое будут находиться призраки в режиме замедления
	{
		if(CallSlowGhostTimer == true)									// Если переменная "CallSlowGhostTimer" равна правда
		{
			if(GhostSlowFreezeTimer > 0)									// Если время в переменной GhostSlowFreezeTime больше ноля
				GhostSlowFreezeTimer -= (1 * Time.deltaTime);		// Отнимаем по еденице каждую секунду
			else if(GhostSlowFreezeTimer <= 0)							// Иначе если время в переменной GhostSlowFreezeTime меньше или равно нулю
			{
				GhostReturnNormalSpeed();									// Вызываем событие GhostReturnNormalSpeed говорящее что призракам будет возвращенна нормальная скорость
				PursuitRunUpTimer = PursuitTime;							// И ставим значение переменной RunUpTimer на 15 секунд
				ToPursuit();													// Вызываем событие переключения призрака в режим "Преследования"
				ActiveModeOfGhosts = ActModOfG.Pursuit;				// Указываем общий для всех режим как "Погоня"
				CallSlowGhostTimer = false;								// Выключаем переменную "CallSlowGhostTimer"
			}
		}
	}


	void GhostFreezingTimer()												// Таймер времени которое будут находиться призраки в режиме заморозки
	{
		if(CallFreezingGhostTimer == true)								// Если переменная "CallFreezingTimer" равна правда
		{
			if(GhostSlowFreezeTimer > 0)									// Если время в переменной GhostSlowFreezeTime больше ноля
				GhostSlowFreezeTimer -= (1 * Time.deltaTime);		// Отнимаем по еденице каждую секунду
			else if(GhostSlowFreezeTimer <= 0)							// Иначе если время в переменной GhostSlowFreezeTime меньше или равно нулю
			{
				GhostReturnNormalSpeed();									// Вызываем событие GhostReturnNormalSpeed говорящее что призракам будет возвращенна нормальная скорость
				PursuitRunUpTimer = PursuitTime;							// И ставим значение переменной RunUpTimer на  PursuitTime
				ToPursuit();													// Вызываем событие переключения призрака в режим "Преследования"
				ActiveModeOfGhosts = ActModOfG.Pursuit;				// Указываем общий для всех режим как "Погоня"
				CallFreezingGhostTimer = false;							// Выключаем переменную "CallFreezingGhostTimer"
			}
		}
	}


	void MethodRunUpTimer()																			// Этот таймер отсчитывает время действия режима разбегания
	{
		if(ActiveModeOfGhosts == ActModOfG.Pursuit && PursuitRunUpTimer > 0)			// Если общий для всех режим это режим преследования и время переменной RunUpTimer больше ноля
		{
			PursuitRunUpTimer -= (1 * Time.deltaTime);										// То отнимаем по еденице в секунду
			if(PursuitRunUpTimer <= 0)																// Если в результате уменьшения RunUpTimer стала равна нулю или меньше
			{
				PursuitRunUpTimer = RunUpTime;													// И ставим значение переменной RunUpTimer на 5 секунд
				ToRunUp();																				// То вызываем событие переключения призрака в режим "Разбегания"
				ActiveModeOfGhosts = ActModOfG.RunUp;											// Указываем общий для всех режим как "Разбегание"
			}
		}
		else if(ActiveModeOfGhosts == ActModOfG.RunUp && PursuitRunUpTimer > 0)		// Иначе если общий для всех режим это режим разбегания и время переменной RunUpTimer больше ноля
		{
			PursuitRunUpTimer -= (1 * Time.deltaTime);										// То отнимаем по еденице в секунду
			if(PursuitRunUpTimer <= 0)																// Если в результате уменьшения RunUpTimer стала равна нулю или меньше
			{
				PursuitRunUpTimer = PursuitTime;													// И ставим значение переменной RunUpTimer на 15 секунд
				ToPursuit();																			// То вызываем событие переключения призрака в режим "Преследования"
				ActiveModeOfGhosts = ActModOfG.Pursuit;										// Указываем общий для всех режим как "Погоня"
			}
		}
	}


	void GatesOpening()																// Метод открытия врат
	{
		for(byte a = 0; a<2; a++)													// Проходим цикл для двух ворот
		{
			Gates[a].transform.GetChild(0).gameObject.SetActive(false);	// Выключаем стенку ворот
			Gates[a].transform.GetChild(2).gameObject.SetActive(true);	// Активируем анимированный спрайт портала
			Gates[a].GetComponent<BoxCollider>().enabled = false;			// Выключаем коллайдеры мешающие пекмену зайти в 1 врата
		}
	}


	void GateClosing()																// Метод закрытия врат
	{
		for(byte a = 0; a<2; a++)													// Проходим цикл для двух ворот
		{
			Gates[a].transform.GetChild(0).gameObject.SetActive(true);	// Включаем стенку ворот
			Gates[a].transform.GetChild(2).gameObject.SetActive(false);	// Деактивируем анимированный спрайт портала
			Gates[a].GetComponent<BoxCollider>().enabled = true;			// Включаем коллайдеры мешающие пекмену зайти в 1 врата
		}																				
	}


	void RoundEndCheck()														// Проверка конца раунда
	{
		if(DotsLeft <= 0 && !LaunchingAllDotsAreCollected) 		// Если количество оставшихся для собирания точек меньше или равно нулю и переменная "LaunchingAllDotsAreCollected" равна ложь
		{
			LaunchingAllDotsAreCollected = true;						// Ставим переменную "LaunchingAllDotsAreCollected" равной правда
			AllDotsAreCollected();											// Запускаем событие "Все точки собранны"
		}
	}


	//---------------------------------------------------------------------------------------------------------------- События ----------------------------------------------------------------------------------------------------------------------------------		


	public void StartGameStarter()								// Метод вызывающий событие старта игры
	{
		StartGame();													// Вызываем событие старта игры
	}
	void MethodStartGame()											// Метод подписанный на событие старта игры
	{
		PlayGame = true;												// Ставим что режим игры активирован
	}


	public void ScreenNowPortraitStarter()						// Метод вызывающий событие "Экран теперь в режиме Portrait"
	{
		ScreenNowPortrait();											// Вызываем событие "Экран теперь в режиме Portrait"
	}


	public void ScreenNowLandscapeStarter()					// Метод вызывающий событие "Экран теперь в режиме Landscape"
	{
		ScreenNowLandscape();										// Вызываем событие "Экран теперь в режиме Landscape"
	}


	public void CherryWasTakenStarter()							// Метод вызывающий событие "Вишня была взята"
	{
		CherryWasTaken();												// Вызываем событие "Вишня была взята"
	}
	void MethodCherryWasTaken()									// Метод вызванный событием "Вишня была взята"								
	{
		_PacManDangerTimer = CherryTime;							// Выставляем таймер сколько пекмен будет опасен
		ActiveModeOfGhosts = ActModOfG.Fright;					// Указываем что пекмен опасен								
		PacManDanger = true;											// Указываем что нужно начинать отсчитывать время опасности пекмена
	}


	public void DecelerationWasTakenStarter()					// Метод вызывающий событие "Замедление было взято"
	{
		DecelerationWasTaken();										// Вызываем метод "Замедление было взято"
	}
	void MethodDecelerationWasTaken()							// Метод вызванный событием "Замедление было взято"	
	{
		if(PacManDanger)
		{
			PursuitRunUpTimer = PursuitTime;						// Ставим значение переменной RunUpTimer на 15 секунд
			ToPursuit();												// Вызываем событие переключения призрака в режим "Преследования"
			ActiveModeOfGhosts = ActModOfG.Pursuit;			// Указываем общий для всех режим как "Погоня"
			PacManDanger = false;									// Указываем переменной "CallPacManTimer" что пока что больше не надо отсчитывать время
		}
		GhostSlowFreezeTimer = SlowTime;							// Выставлем таймер сколько секунд приведения будут замедленны
		CallSlowGhostTimer = true;									// Указываем что нужно начинать отсчитывать время режима замедления приведений
	}


	public void FreezingWasTakenStarter()						// Метод вызывающий событие "Заморозка была взята"
	{
		Debug.Log("Заморозка была взята");
		FreezingWasTaken();											// Вызываем метод "Заморозка была взята"
	}
	void MethodFreezingWasTaken()									// Метод вызванный событием "Заморозка была взята"	
	{
		if(PacManDanger)
		{
			PursuitRunUpTimer = PursuitTime;						// Ставим значение переменной RunUpTimer на 15 секунд
			ToPursuit();												// Вызываем событие переключения призрака в режим "Преследования"
			ActiveModeOfGhosts = ActModOfG.Pursuit;			// Указываем общий для всех режим как "Погоня"
			PacManDanger = false;									// Указываем переменной "CallPacManTimer" что пока что больше не надо отсчитывать время
		}

		GhostSlowFreezeTimer = FreezingTime;					// Выставлем таймер сколько секунд приведения будут замороженны
		CallFreezingGhostTimer = true;							// Указываем что нужно начинать отсчитывать время режима заморозки приведений
	}
		

	void MethodPacManNotDanger()									// Этот метод вызываеться событием "Пекмен больше не опасен"
	{
		ActiveModeOfGhosts = ActModOfG.Pursuit;				// Указываем что призрак снова может переходить в режим преследования
		PacManDanger = false;										// Указываем что больше не нужно отсчитывать время опасности пекмена
	}


	public void KillPacManStarter()								// Метод вызывающий событие "Убить пекмена"
	{
		KillPacMan();													// Вызываем метод "Убить пекмена"
	}
	void MethodKillPacMan()											// Метод вызванный событием "Убить пекмена"	
	{
		PlayGame = false;												// Ставим игру на паузу
		PacmanLifes--;													// Отнимаем одну жизнь из жизней пекмена
		GamePoints -= 10;												// Отнимаем 10 очков за смерть
		if(GamePoints < 0)											// Если количество очков пемена ушло в минус
		{
			GamePoints = 0;											// Ставим количество очков как 0
		}
		if(PacmanLifes >= 0)											// Если количество жизней пемена больше ноля
		{
			GUInt.PortraitPacManLifes.transform.GetChild(PacmanLifes).gameObject.GetComponent<Image>().sprite = GUInt.PacManIcons[0];	// Ставим пустой значёк жизней пекмена для "Portrait" интерфейса
			GUInt.LandscapePacManLifes.transform.GetChild(PacmanLifes).gameObject.GetComponent<Image>().sprite = GUInt.PacManIcons[0];	// Ставим пустой значёк жизней пекмена для "Landscape" интерфейса
		}
		GUInt.PortraitGamePoints.text = GamePoints.ToString();			// Переделываем числовое значение очков пекмена в строковое и передаём на экран для "Portrait" интерфейса
		GUInt.LandscapeGamePoints.text = GamePoints.ToString();			// Переделываем числовое значение очков пекмена в строковое и передаём на экран для "Landscape" интерфейса
		GUInt.PortraitGameOver.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = GamePoints.ToString(); 	// Передаём строке значение общего количества очков пекмена которая отображает их в конце игры для "Portrait" интерфейса
		GUInt.LandscapeGameOver.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = GamePoints.ToString();	// Передаём строке значение общего количества очков пекмена которая отображает их в конце игры для "Landscape" интерфейса
		StartCoroutine(IEnumKillPacMan());										// Вызываем корутину "IEnumKillPacMan"
	}
	IEnumerator IEnumKillPacMan()													// Этот метод-корутина продолжение метода "MethodKillPacMan"
	{
		if(PacmanLifes >= 0) 														// Если количество жизней пемена больше ноля
		{
			yield return new WaitForSeconds(3);									// Ждём 3 секунды
			PlayGame = true;															// Снимаем игру с паузы
		}
	}


	void MethodAllDotsAreCollected()												// Метод подписанный на событие "Все точки собранны"
	{
		StartCoroutine(IEnumAllDotsAreCollected());							// Вызываем Метод способный отсчитывать время	
	}
	IEnumerator IEnumAllDotsAreCollected()										// Этот метод-корутина продолжение метода "MethodAllDotsAreCollected"
	{
		if(GameLevel < 10)															// Если это был не последний уровень
		{
			PlayGame = false;															// Ставим на паузу режим игры
			float Variable = 0;														// Переменная отвечающая за изменение цвета
			GameLevel++;																// Ставим следующий уровень
			DotsLeft = 380;															// Ставим стандартное для начала количество точек которые необходимо собрать
			DotsEaten = 0;																// Ставим нулевое значение съеденных дотов в этом новом уровне
			LaunchingAllDotsAreCollected = false;								// Ставим переменную "LaunchingAllDotsAreCollected" равной ложь чтобы на следующем уровне опять можно было закончить уровень
			GhostSlowFreezeTimer = 0;												// Ставим счётчик отсчитывающий оставшееся время замедления или заморозки на 0 чтобы приведение не имело штрафов на скорость при старте нового уровня
			PacManNotDanger();														// Вызываем событие PacManNotDanger говорящее что пекмен больше не опасен
			yield return new WaitForSeconds(2);									// Ждём
			NewLevelEvents();															// Вызываем метод определяющий какие события должны произойти при старте этого конкретного уровня
			PacDots.name = "PacDots";												// Называем эту группу как "PacDots"
			for(byte a=0; RealMat.color != Mat[GameLevel].color; a++)	// Проходим нужное количество циклов пока цвет не достигнет желаемого
			{
				RealMat.color = Color.Lerp(Mat[GameLevel-1].color, Mat[GameLevel].color, Variable);	// Изменяем цвет лабиринта на определённый шаг от предыдущего цвета к целевому
				yield return new WaitForSeconds (0.07f);						// Ждём
				Variable += 0.06f;													// Прибавляем к variable значение
			}
			yield return new WaitForSeconds(3);									// Ждём
			PursuitRunUpTimer = PursuitTime;										// И ставим значение переменной PursuitRunUpTimer на PursuitTime чтобы счётчик погони был полным
			ToPursuit();																// Вызываем событие переключения призрака в режим "Преследования" чтобы все приведения что не находяться дома выбрали режим погони
			ActiveModeOfGhosts = ActModOfG.Pursuit;							// Указываем общий для всех режим как "Погоня" чтобы запустить общий для всех приведений счётчик погони
			PlayGame = true;															//	Снимаем с паузы режим игры
			StartLevel();																// Вызываем событие старта уровня
		}
		else 																				// Иначе если это был последний уровень
		{
			GameLevel++;																// Ставим следующий уровень (Для того чтобы прошла уцспешно проверка на конец игры и пекмен ну убралься на место в скрипте Pacmanmmove раньше чем погаснет экран)
			PlayGame = false;															// Ставим на паузу режим игры
			EndGame();																	// Вызываем событие конца игры
		}
	}
	void NewLevelEvents() 															// Этот метод выбирает что произойдёт при переходе пекмена на новый уровень
	{
		string PD = "PacDots (" + GameLevel + ")";							// Строковая переменная с названием префаба пекдотов с текщим уровнем
		GameObject.Destroy(PacDots);												// Удаляем группу старых пекдотов
		PacDots = Instantiate(Resources.Load(PD) as GameObject);			// Телепортируем на сцену новую группу пекдотов с вишенкой и ложим её в GM.PacDots

		switch(GameLevel)
		{
		case 4:																						// Если мы загружаем 4 уровень
			GatesOpening();																		// Вызываем метод запускающий открытие врат
			break;
		}
	}


	public void NewGameStarter()																// Этот метод вызывает событие "Новая игра"		
	{
		NewGame();																					// Вызываем событие новой игры
	}
	void MethodNewGame()																			// Этот метод вызываеться событием "Новая игра"
	{
		GameObject.Destroy(PacDots);															// Удаляем группу старых пекдотов
		PacDots = Instantiate(Resources.Load("PacDots (1)") as GameObject);		// Телепортируем на сцену новую группу пекдотов и ложим её в GM.PacDots
		PacDots.name = "PacDots";																// Называем эту группу как "PacDots"
		PacmanLifes = 2;																			// Ставим пекмену опять 3 жизни
		DotsLeft = 380;																			// Ставими начально количество оставшихся пекдотов													
		DotsEaten = 0;																				// Обнуляем его собранные пекдоты
		GamePoints = 0;																			// Обнуляем количество набранных очков
		GameLevel = 1;     																		// Ставим заного первый уровень
		LaunchingAllDotsAreCollected = false;												// Ставим переменную "LaunchingAllDotsAreCollected" равной ложь чтобы на следующем уровне опять можно было закончить уровень
		RealMat.color = Mat[GameLevel].color;												// В начале ставим материал лабиринта как первый
		GateClosing();																				// Вызываем метод закрытия врат
		StartCoroutine(NewGamePlus());														// Вызываем коронтину NewGamePlus()
	}
	IEnumerator NewGamePlus()																	// Этот метод продолжение метода "Новая игра" но способный отсчитывать время
	{
		yield return new WaitForSeconds(1);													// Ждём одну секунду	
		PlayGame = true;																			// Снимаем режи игры с паузы
	}


	public void PauseStarter()											// Этот метод запускает событие "Пауза"
	{
		Pause();																// Вызываем событие "Пауза"
	}
	void MethodPause()													// Этот метод подписан на событие "Пауза"
	{
		PlayGame = false;													// Указываем в переменной PlayGame что игра на паузе								
		Time.timeScale = 0;												// Стави скорость игры на паузу
	}


	public void PlayStarter()											// Этот метод запускает событие "Снять с паузы"
	{
		Play(); 																// Вызываем событие "Снять с паузы"
	}
	void MethodPlay()														// Этот метод подписан на событие "Снять с паузы"
	{
		PlayGame = true;													// Указываем в переменной PlayGame что игра снята с паузы	
		Time.timeScale = 1;												// Стави скорость игры в норму
	}


	void OnDisable()
	{
		StartGame -= MethodStartGame;									// Отписываем метод "MethodStartGame" от события "StartGame" 
		CherryWasTaken -= MethodCherryWasTaken;					// Отписываем метод "MethodCherryWasTaken" от события "CherryWasTaken"
		DecelerationWasTaken -= MethodDecelerationWasTaken;	// Отписываем метод "MethodDecelerationWasTaken" от события "DecelerationWasTaken"
		FreezingWasTaken -= MethodFreezingWasTaken;				// Отписываем метод "MethodFreezingWasTaken" от события "FreezingWasTaken"
		PacManNotDanger -= MethodPacManNotDanger;					// Отписываем метод "PacManNotDanger" от события "MethodPacManNotDanger"
		KillPacMan -= MethodKillPacMan;								// Отписываем метод "MethodKillPacMan" от события "KillPacMan"
		AllDotsAreCollected -= MethodAllDotsAreCollected;		// Отписываем метод "MethodAllDotsAreCollected" от события "Все точки собранны" 
		Pause -= MethodPause;											// Отписываем метод "MethodPause" от события "Pause"
		Play -= MethodPlay;												// Отписываем метод "MethodPlay" от события "Play"
		NewGame -= MethodNewGame;										// Отписываем метод "MethodNewGame" от события "NewGame"
	}
}
