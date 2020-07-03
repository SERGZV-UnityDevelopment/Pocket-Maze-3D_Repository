using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// Памятка какие кнопки есть на андроид
/*
 	if (Input.GetKey(KeyCode.Home) || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Menu))
	{
		Application.Quit();
		return;
	}
*/


public class GUInterface : MonoBehaviour 
{
	public Camera MainCamera;							// Переменная для камеры
	public GameObject MainMenu;						// Переменная для канваса главного меню
	public PacManMove PM;								// Переменая для объекта скрипта PacManMove
	public Text PortraitDotsLeftT;					// Текст указывающий количество дотов которые осталось собрать для "Portrait" интерфейса
	public Text LandscapeDotsLeft;					// Текст указывающий количество дотов которые осталось собрать для "Landscape" интерфейса
	public Text PortraitGamePoints;					// Текст указывающий количество очков которые набрал пекмен для "Portrait" интерфейса
	public Text LandscapeGamePoints;					// Текст указывающий количество очков которые набрал пекмен для "Landscape" интерфейса
	public Vector3 PortraitCameraPos;				// Позиция камеры для режима экрана "Portrait"		
	public Vector3 PortraitCameraRot;				// Вращение камеры для режима экрана "Portrait"
	public Vector3 LandscapeCameraPos;				// Позиция камеры для режима экрана "Landscape"
	public Vector3 LandscapeCameraRot;				// Вращение камеры для режима экрана "Landscape"
	public GameObject PortraitPacManLifes;			// Объект PacManLifes который содержит в себе компонент UI (Panel) для "Portrait" интерфейса
	public GameObject LandscapePacManLifes;		// Объект PacManLifes который содержит в себе компонент UI (Panel) для "Landscape" интерфейса
	public GameObject PortraitControlOfPacman;	// Панель содержащая кнопки контроля пекменом для "Portrait" интерфейса
	public GameObject LandscapeControlOfPacman;	// Панель содержащая кнопки контроля пекменом для "Landscape" интерфейса
	public Sprite[] PacManIcons;						// Массив из двух спрайтов-иконок первая пустая вторая с пекменом
	public GameObject PortraitGameOver;				// Здесь находиться панель отвечающая за окно "GameOver" для "Portrait" интерфейса
	public GameObject LandscapeGameOver;			// Здесь находиться панель отвечающая за окно "GameOver" для "Landscape" интерфейса
	GameManager GM;										// Скрипт Гейм менеджера
	GameObject _Panel_;									// Главная панель канваса 
	GameObject PortraitInterface;						// Здесь находиться панель отвечающая за весь "Портрет" интерфейс
	GameObject LandscapeInterface;					// Здесь находиться панель отвечающая за весь "Пейзажный" интерфейс
	GameObject PortraitPauseText;						// Здесь находиться панель отвечающая за текст предупреждения выхода из игры для "Portrait" интерфейса
	GameObject LandscapePauseText;					// Здесь находиться панель отвечающая за текст предупреждения выхода из игры для "Landscape" интерфейса
	GameObject PortraitShading;						// Затемнение для "Portrait" интерфейса	
	GameObject LandscapeShading;						// Затемнение для "Landscape" интерфейса
	bool ScreenPortrait;									// Если правда то экран в режиме Portrait
	bool ScreenLandscape;								// Если правда то экран в режиме Landscape


	void OnEnable()										// При активации скрипта
	{
		GameManager.StartGame += MethodStartGame;									// Подписываем метод "MethodStartGame" на cобытие "StartGame"
		GameManager.ScreenNowPortrait += MethodScreenNowPortrait;			// Подписываем метод "MethodScreenNowPortrait" на cобытие "ScreenNowPortrait"
		GameManager.ScreenNowLandscape += MethodScreenNowLandscape;			// Подписываем метод "MethodScreenNowLandscape" на cобытие "ScreenNowLandscape"
		GameManager.KillPacMan += MethodKillPacMan;								// Подписываем метод "MethodKillPacMan" на событие "KillPacMan"
		GameManager.AllDotsAreCollected += MethodAllDotsAreCollected;		// Подписываем метод "MethodAllDotsAreCollected" на cобытие "Все точки собранны" 
		GameManager.NewGame += MethodNewGame;										// Подписываем метод "MethodNewGame" на событие "NewGame"
		GameManager.Pause += MethodPause;											// Подписываем метод "MethodPause" на событие "Pause"
		GameManager.Play += MethodPlay;												// Подписываем метод "MethodPlay" на событие "Play"
		GameManager.EndGame += MethodEndGame;										// подписываем метод "MethodEndGame" на событие "EndGame"
	}


	public void Start()																	// Этот метод вызываеться при старте игры
	{
		GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();										// Находим скрипт гейм менеджера и ложим его в "GM"
		PortraitPacManLifes = MainMenu.transform.GetChild(0).GetChild(2).GetChild(2).gameObject;								// Заносим в PacManLifes GameObject представляющий UI панель для "Portrait" интерфейса
		LandscapePacManLifes = MainMenu.transform.GetChild(1).GetChild(2).GetChild(2).gameObject;								// Заносим в PacManLifes GameObject представляющий UI панель для "Landscape" интерфейса
		PortraitInterface = MainMenu.transform.GetChild(0).gameObject;																	// Заносим панель "PortraitInterface" в переменную "PortraitInterface"
		LandscapeInterface = MainMenu.transform.GetChild(1).gameObject;																// Заносим панель "LandscapeInterface" в переменную "LandscapeInterface"
		PortraitControlOfPacman = MainMenu.transform.GetChild(0).GetChild(1).gameObject;											// Заносим панель на которой висят кнопки управления пекменом в переменную "ControlOfPacman" для "Portrait" интерфейса
		LandscapeControlOfPacman = MainMenu.transform.GetChild(1).GetChild(1).gameObject;										// Заносим панель на которой висят кнопки управления пекменом в переменную "ControlOfPacman" для "Landscape" интерфейса
		PortraitGameOver = MainMenu.transform.GetChild(0).GetChild(3).gameObject;													// Заносим панель GameOver для "Portrait" интерфейса в переменную "PortraitGameOver"
		LandscapeGameOver = MainMenu.transform.GetChild(1).GetChild(3).gameObject;													// Заносим панель GameOver для "Landscape" интерфейса в переменную "LandscapeGameOver"
		PortraitPauseText = MainMenu.transform.GetChild(0).GetChild(4).gameObject;													// Заносим текст предупреждения выхода из игры для "Portrait" интерфейса в переменную PortraitPauseText
		LandscapePauseText = MainMenu.transform.GetChild(1).GetChild(4).gameObject;												// Заносим текст предупреждения выхода из игры для "Landscape" интерфейса в переменную LandscapePauseText
		PortraitShading = MainMenu.transform.GetChild(0).GetChild(6).gameObject;													// Заносим изображение затенения для "Portrait" интерфейса в переменную PortraitShading
		LandscapeShading = MainMenu.transform.GetChild(1).GetChild(6).gameObject;													// Заносим изображение затенения для "Landscape" интерфейса  в переменную LandscapeShading
		PortraitGameOver.transform.GetChild(0).GetComponent<Image>().color = (Color32)new Color(250, 250, 250, 0);		// Ставим прозрачность изображения  "GameOver" для "Portrait" интерфейса на "0"
		PortraitGameOver.transform.GetChild(2).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);		// Ставим прозрачность изображения "YouWin!" для "Portrait" интерфейса на "0"
		LandscapeGameOver.transform.GetChild(0).GetComponent<Image>().color = (Color32)new Color(250, 250, 250, 0);		// Ставим прозрачность изображения "GameOver" для "Landscape" интерфейса на "0"
		LandscapeGameOver.transform.GetChild(2).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);		// Ставим прозрачность изображения "YouWin!" для "Landscape" интерфейса на "0"
		PortraitDotsLeftT = MainMenu.transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<Text>();					// Присваиваем объект гуи панели текст указывающий количество оставшихся дотов "PortraitDotsLeftT"
		LandscapeDotsLeft = MainMenu.transform.GetChild(1).GetChild(2).GetChild(1).GetComponent<Text>();					// Присваиваем объект гуи панели текст указывающий количество оставшихся дотов "LandscapeDotsLeft"
		PortraitGamePoints = MainMenu.transform.GetChild(0).GetChild(2).GetChild(3).GetChild(1).GetComponent<Text>();	// Присваиваем объект гуи панели текст указывающий количество набранных пекменом очков переменной "PortraitGamePoints"
		LandscapeGamePoints = MainMenu.transform.GetChild(1).GetChild(2).GetChild(3).GetChild(1).GetComponent<Text>();	// Присваиваем объект гуи панели текст указывающий количество набранных пекменом очков переменной "LandscapeGamePoints"
		PortraitInterface.transform.GetChild(0).gameObject.SetActive(true);															// Включаем панель старта игры для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(0).gameObject.SetActive(true);															// Включаем панель старта игры для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(1).gameObject.SetActive(false);															// Отключаем панель кнопок управления пекменом для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(1).gameObject.SetActive(false);														// Отключаем панель кнопок управления пекменом для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(false);															// Отключаем панель очков игрока для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(false);														// Отключаем панель очков игрока для "Landscape" интерфейса
		PortraitGameOver.SetActive(false);																										// Отключаем панель GameOver для "Portrait" интерфейса
		LandscapeGameOver.SetActive(false);																										// Отключаем панель GameOver для "Landscape" интерфейса
		PortraitGameOver.transform.GetChild(1).gameObject.SetActive(false);															// Отключаем панель кнопок новая игра/выход для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(1).gameObject.SetActive(false);															// Отключаем панель кнопок новая игра/выход для "Landscape" интерфейса
		PortraitShading.SetActive(false);																										// Отключаем панель затенения для "Portrait" интерфейса
		LandscapeShading.SetActive(false);																										// Отключаем панель затенения для "Landscape" интерфейса
		PortraitPauseText.SetActive(false);																										// Скрываем текст предупреждения выхода из игры для "Portrait" интерфейса
		LandscapePauseText.SetActive(false);																									// Скрываем текст предупреждения выхода из игры для "Landscape" интерфейса
	}


	void Update()																	// Вызываем каждый кадр
	{
		ScreenRotationCheck();													// Вызываем проверку на поворот экрана

		if(GM.PlayGame == true)													// Если переменная "PlayGame" равна правда
		{
			if(Input.GetKey(KeyCode.Escape))									// Если нажата клавиша "Escape"
			{
				GM.PauseStarter();												// Вызываем метод вызывающий событие "Пауза"
			}
		}
	}


	public void StartButton() 													// Вызываеться при нажатии кнопки "StartButton"
	{
		GM.StartGameStarter();													// Вызываем метод вызываюший событие старта игры			
	}


	public void ExitButton()													// Вызываеться при нажатии кнопок выхода из игры
	{
		Application.Quit();														// Выходим из приложения
	}


	public void Play()															// Вызываеться нажатием кнопки "Play"
	{
		GM.PlayStarter();															// Вызываем событие снятия игры с паузы
	}


	public void Restart()														// Вызываеться нажатием кнопки "Restart"
	{
		GM.NewGameStarter();														// Вызываем метод вызывающий событие новой игры
	}


	public void ScreenRotationCheck()										// Этот метод проверяет в каком из режимов находиться экран Portrait или Landscape и вызывает соответствующее событие
	{
		if(Screen.height>Screen.width && !ScreenPortrait)				// Если высота экрана больше ширины экрана значит мы в режиме Portrait
		{
			GM.ScreenNowPortraitStarter();									// Вызываем метод вызывающий событие "Экран теперь в режиме Portrait"
			ScreenPortrait = true;												// Указываем что переменная "ScreenPortrait" равна правда
			ScreenLandscape = false;											// Указываем что переменная "ScreenLandscape" равна ложь
		}
		if(Screen.height<Screen.width && !ScreenLandscape)				// Иначе если высота экрана меньше ширины экрана значит мы в режиме Landscape
		{
			GM.ScreenNowLandscapeStarter();									// Вызываем метод вызывающий событие "Экран теперь в режиме Landscape"
			ScreenLandscape = true;												// Указываем что переменная "ScreenLandscape" равна правда
			ScreenPortrait = false;												// Указываем что переменная "ScreenPortrait" равна ложь
		}
	}


	//------------------------------------------------------------------------------------------------- События ---------------------------------------------------------------------------------------------------------------------------------------------


	void MethodStartGame()																		// Метод подписанный на событие старта игры
	{
		PortraitInterface.transform.GetChild(0).gameObject.SetActive(false);		// Отключаем стартовое окно для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(0).gameObject.SetActive(false);	// Отключаем стартовое окно для "Landscape" интерфейса
		PortraitControlOfPacman.gameObject.SetActive(true);							// Включаем кнопки управления Пекменом для "Portrait" интерфейса
		LandscapeControlOfPacman.gameObject.SetActive(true);							// Включаем кнопки управления Пекменом для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(true);		// Включаем панель очков игрока
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(true);		// Включаем панель очков игрока
	}
		

	void MethodScreenNowPortrait()															// Метод подписанный на событие "Экран теперь в режиме Portrait"
	{
		MainCamera.transform.position = PortraitCameraPos;								// Выставляем позицию камеры для игры в режиме "Portrait"
		MainCamera.transform.rotation = Quaternion.Euler(PortraitCameraRot);		// Выставляем вращение камеры для игры в режиме "Portrait"
		LandscapeInterface.SetActive(false);												// Отключаем "Landscape" интерфейс
		PortraitInterface.SetActive(true);													// Включаем "Portrait" интерфейс
	}


	void MethodScreenNowLandscape()															// Метод подписанный на событие "Экран теперь в режиме Landscape"
	{
		MainCamera.transform.position = LandscapeCameraPos;							// Выставляем позицию камеры для игры в режиме "LandScape"
		MainCamera.transform.rotation = Quaternion.Euler(LandscapeCameraRot);	// Выставляем вращение камеры для игры в режиме "LandScape"
		PortraitInterface.SetActive(false);													// Отключаем "Portrait" интерфейс
		LandscapeInterface.SetActive(true);													// Включаем "Landscape" интерфейс
	}


	void MethodKillPacMan()																		// Метод вызванный событием "Убить пекмена"	
	{
		for(byte a=0; a<4; a++)																	// 4 раза прогоняем цикл 
		{
			PortraitControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = false;	// Ставим все элементы управления пекменом в режим неактивных для "Portrait" интерфейса
			LandscapeControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = false;	// Ставим все элементы управления пекменом в режим неактивных для "Landscape" интерфейса
		}
		PortraitShading.SetActive(true);																						// Затемняем экран для "Portrait" интерфейса
		LandscapeShading.SetActive(true);																					// Затемняем экран для "Landscape" интерфейса
		StartCoroutine(IEnumKillPacMan());													// Вызываем корутину "IEnumKillPacMan"
	}
	IEnumerator IEnumKillPacMan()																// Этот метод-корутина продолжение метода "MethodKillPacMan"
	{
		yield return new WaitForSeconds(2);													// Ждём 2 секунды	

		if(GM.PacmanLifes >= 0) 																// Если количество жизней пемена больше или равно нулю
		{
			yield return new WaitForSeconds(1);																						// Ждём одну секунду		
			for(byte a = 0; a < 4; a++)																								// 4 раза прогоняем цикл 
			{
				PortraitControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true;			// И ставим все элементы управления пекменом в режим активных для "Portrait" интерфейса
				LandscapeControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true;		// И ставим все элементы управления пекменом в режим активных для "Landscape" интерфейса
			}
			PortraitShading.SetActive(false);																						// Затемняем экран для "Portrait" интерфейса
			LandscapeShading.SetActive(false);																						// Затемняем экран для "Landscape" интерфейса
		}
		else if(GM.PacmanLifes < 0) 																									// Если количество жизней пекмена стало меньше ноля
		{
			MainMenu.transform.GetChild(0).GetChild(3).gameObject.SetActive(true);										// Делаем видимой панель GameOver для "Portrait" интерфейса
			MainMenu.transform.GetChild(1).GetChild(3).gameObject.SetActive(true);										// Делаем видимой панель GameOver для "Landscape" интерфейса
			Image PortraitIm = MainMenu.transform.GetChild(0).GetChild(3).GetChild(0).GetComponent<Image>();	// Достаём компонент Image из изображения GameOver и ложим его в переменную "PortraitIm"
			Image LandscapeIm = MainMenu.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Image>();	// Достаём компонент Image из изображения GameOver и ложим его в переменную "LandscapeIm"
			float Transparency = 0;																										// Переменная отвечающая за прозрачность изображения

			yield return new WaitForSeconds(1);													// Ждём
			for(; Transparency<1;)																	// Проходим циклы постепенно делая текст "GameOver" Видимым
			{
				Transparency += 0.05f;																// Прибавляем прозрачность переменной Transparency
				yield return new WaitForSeconds(0.06f);										// Ждём
				PortraitIm.color = (Color32)new Color(250, 250, 250, Transparency);	// Присваиваем новую прозрачность изображению "GameOver" для "Portrait" интерфейса
				LandscapeIm.color = (Color32)new Color(250, 250, 250, Transparency);	// Присваиваем новую прозрачность изображению "GameOver" для "Landscape" интерфейса
			}
			yield return new WaitForSeconds(1);													// Ждём
			MainMenu.transform.GetChild(0).GetChild(3).GetChild(1).gameObject.SetActive(true);	// Делаем панель с выбором кнопок главного меню активными для "Portrait" интерфейса
			MainMenu.transform.GetChild(1).GetChild(3).GetChild(1).gameObject.SetActive(true);	// Делаем панель с выбором кнопок главного меню активными для "Landscape" интерфейса
			PortraitGameOver.transform.GetChild(3).gameObject.SetActive(true); 		// Делаем активной панель показывающую общее количество очков пекмена для "Portrait" интерфейса
			LandscapeGameOver.transform.GetChild(3).gameObject.SetActive(true); 		// Делаем активной панель показывающую общее количество очков пекмена для "Landscape" интерфейса
		}
	}

	void MethodAllDotsAreCollected()														// Этот метод запускает корутину завершения уровня
	{
		if(GM.GameLevel < 10)																// Если это был не последний уровень
		{
			StartCoroutine(IEnumAllDotsAreCollected());								// Вызываем Метод способный отсчитывать время	
		}
	}
	IEnumerator IEnumAllDotsAreCollected()												// Этот метод-корутина продолжение метода "MethodAllDotsAreCollected"
	{
		float Variable = 0;																											// Переменная отвечающая за изменение цвета 
		Image PortraitI;																												// Временная переменная для изображения слова "Level" для "Portrait" интерфейса
		Image LandscapeI;																												// Временная переменная для изображения слова "Level" для "Landscape" интерфейса
		Text PortraitT;																												// Временная переменная для цифры указывающей текущий уровень в начале уровня для "Portrait" интерфейса
		Text LandscapeT;																												// Временная переменная для цифры указывающей текущий уровень в начале уровня для "Landscape" интерфейса
		Color IC = PortraitInterface.transform.GetChild(7).GetChild(0).GetComponent<Image>().color;			// Цвет изображения для "Portrait" интерфейса
		Color TC = PortraitInterface.transform.GetChild(7).GetChild(1).GetComponent<Text>().color;			// Цвет текста для "Portrait" интерфейса

		for(byte a=0; a<4; a++)																										// 4 раза прогоняем цикл и ставим все элементы управления пекменом в режим неактивных
		{
			PortraitControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = false;		// Отключаем элементы управления пекменом для "Portrait" интерфейса
			LandscapeControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = false;		// Отключаем элементы управления пекменом для "Landscape" интерфейса
		}
		PortraitShading.SetActive(true);																							// Затемняем экран для "Portrait" интерфейса
		LandscapeShading.SetActive(true);																						// Затемняем экран для "Landscape" интерфейса

		PortraitI = PortraitInterface.transform.GetChild(7).GetChild(0).GetComponent<Image>();					// Ложим изображение слова "Level" в переменную "PortraitI"
		LandscapeI = LandscapeInterface.transform.GetChild(7).GetChild(0).GetComponent<Image>();				// Ложим изображение слова "Level" в переменную "LandscapeI"
		PortraitT = PortraitInterface.transform.GetChild(7).GetChild(1).GetComponent<Text>();					// Ложим текст указывающий текущий уровень в переменную "PortraitT"
		LandscapeT = LandscapeInterface.transform.GetChild(7).GetChild(1).GetComponent<Text>();				// Ложим текст указывающий текущий уровень в переменную "LandscapeT"

		PortraitI.color = new Color(IC.r, IC.g, IC.b, 0);																	// Ставим альфа прозрачность цвета текста 0 для "Portrait" интерфейса
		LandscapeI.color = new Color(TC.r, TC.g, TC.b, 0);																	// Ставим альфа прозрачность цвета цифры 0 для "Landscape" интерфейса
		PortraitT.color = new Color(TC.r, TC.g, TC.b, 0);																	// Ставим альфа прозрачность цвета цифры 0 для "Portrait" интерфейса
		LandscapeT.color = new Color(TC.r, TC.g, TC.b, 0);																	// Ставим альфа прозрачность цвета цифры 0 для "Landscape" интерфейса

		yield return new WaitForSeconds(3); 																					// Ждём
		PortraitT.text = GM.GameLevel.ToString(); 																			// Переводим номер уровня в текст и присваиваем эту цифру переменной "PortraitT"
		LandscapeT.text = GM.GameLevel.ToString(); 																			// Переводим номер уровня в текст и присваиваем эту цифру переменной "LandscapeT"

		PortraitDotsLeftT.text = GM.DotsLeft.ToString();																	// Сколько осталось дотов переделываем числовое значение в строковое и передаём на экран для "Portrait" интерфейса
		LandscapeDotsLeft.text = GM.DotsLeft.ToString();																	// Сколько осталось дотов переделываем числовое значение в строковое и передаём на экран для "Landscape" интерфейса

		PortraitInterface.transform.GetChild(7).gameObject.SetActive(true);											// Делаем видимой категорию таблички указывающей номер уровня для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(7).gameObject.SetActive(true);											// Делаем видимой категорию таблички указывающей номер уровня для "Landscape" интерфейса



		for(byte a=0; PortraitI.color.a < 1.0f; a++)																			// Проходим нужное количество циклов пока прозрачность не достигнет желаемого уровня
		{
			PortraitI.color = Color.Lerp(new Color(IC.r, IC.g, IC.b, 0), new Color(IC.r, IC.g, IC.b, 1), Variable);	// Изменяем цвет текста на определённый шаг от предыдущего цвета к целевому для "Portrait" интерфейса
			LandscapeI.color = PortraitI.color;																								// Копируем цвет текста с "Portrait" интерфейса для "Landscape" интерфейса
			PortraitT.color = Color.Lerp(new Color(TC.r, TC.g, TC.b, 0), new Color(TC.r, TC.g, TC.b, 1), Variable);	// Изменяем цвет цифры на определённый шаг от предыдущего цвета к целевому
			LandscapeT.color = PortraitT.color;																								// Копируем цвет цифры с "Portrait" интерфейса для "Landscape" интерфейса
			yield return new WaitForSeconds (0.07f);																			// Ждём
			Variable += 0.06f;																										// Прибавляем к variable значение
		}

		yield return new WaitForSeconds(1.5f);																					// Ждём
		PortraitInterface.transform.GetChild(7).gameObject.SetActive(false);											// Делаем невидимой панель "LevelLabel" для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(7).gameObject.SetActive(false);										// Делаем невидимой панель "LevelLabel" для "Landscape" интерфейса

//		PortraitInterface.GetComponent<Image>().color = new Color32(0,0,0,0);										// Делаем экран снова прозрачным (ставим видимость PortraitInterface 0)
//		LandscapeInterface.GetComponent<Image>().color = new Color32(0,0,0,0);										// Делаем экран снова прозрачным (ставим видимость LandscapeInterface 0)

		PortraitShading.SetActive(false);																						// Затемняем экран для "Portrait" интерфейса
		LandscapeShading.SetActive(false);																						// Затемняем экран для "Landscape" интерфейса

		for(byte a=0; a<4; a++)																										// 4 раза прогоняем цикл и ставим все элементы управления пекменом в режим активных
		{
			PortraitControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true;			// Включаем элементы управления пекменом для "Portrait" интерфейса
			LandscapeControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true;		// Включаем элементы управления пекменом для "Landscape" интерфейса
		}
	}


	void MethodNewGame()																															// Этот метод вызываеться событием "Новая игра"
	{
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(true);														// Включаем панель очков пекмена	для "Portrait" интерфейса			 						
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(true);														// Включаем панель очков пекмена	для "Landscape" интерфейса
		PortraitControlOfPacman.SetActive(true);																							// Включаем панель кнопок управления пекменом для "Portrait" интерфейса
		LandscapeControlOfPacman.SetActive(true);																							// Включаем панель кнопок управления пекменом для "Landscape" интерфейса
		PortraitPacManLifes.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = PacManIcons[1];			// Меняем 1 пустую рамку на значёк жизней пекмена для "Portrait" интерфейса
		PortraitPacManLifes.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = PacManIcons[1];			// Меняем 2 пустую рамку на значёк жизней пекмена для "Portrait" интерфейса
		LandscapePacManLifes.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = PacManIcons[1];			// Меняем 1 пустую рамку на значёк жизней пекмена для "Landscape" интерфейса
		LandscapePacManLifes.transform.GetChild(1).gameObject.GetComponent<Image>().sprite = PacManIcons[1];			// Меняем 2 пустую рамку на значёк жизней пекмена для "Landscape" интерфейса
		MainMenu.transform.GetChild(0).GetChild(3).gameObject.SetActive(false);													// Делаем не видимой панель GameOver для "Portrait" интерфейса
		MainMenu.transform.GetChild(1).GetChild(3).gameObject.SetActive(false);													// Делаем не видимой панель GameOver для "Landscape" интерфейса
		MainMenu.transform.GetChild(0).GetChild(3).GetChild(1).gameObject.SetActive(false);									// Делаем панель с выбором кнопок главного меню не активными для "Portrait" интерфейса
		MainMenu.transform.GetChild(1).GetChild(3).GetChild(1).gameObject.SetActive(false);									// Делаем панель с выбором кнопок главного меню не активными для "Landscape" интерфейса
		PortraitGameOver.transform.GetChild(0).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);	// Ставим прозрачность изображения "GameOver" 0 для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(0).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);	// Ставим прозрачность изображения "GameOver" 0 для "Landscape" интерфейса
		PortraitGameOver.transform.GetChild(2).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);	// Ставим прозрачность изображения "YouWin!" 0 для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(2).GetComponent<Image>().color = (Color32)new Color(255, 255, 255, 0);	// Ставим прозрачность изображения "YouWin!" 0 для "Landscape" интерфейса
		PortraitGameOver.transform.GetChild(3).gameObject.SetActive(false); 														// Делаем не активной панель показывающую общее количество очков пекмена для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(3).gameObject.SetActive(false); 													// Делаем не активной панель показывающую общее количество очков пекмена для "Landscape" интерфейса
		StartCoroutine(NewGamePlus());																										// Вызываем коронтину NewGamePlus()
	}
	IEnumerator NewGamePlus()																// Этот метод продолжение метода "Новая игра" но способный отсчитывать время
	{
		yield return new WaitForSeconds(1);												// Ждём одну секунду	
		PortraitDotsLeftT.text = GM.DotsLeft.ToString();							// Переделываем числовое значение в строковое и передаём на экран	для "Portrait" интерфейса
		LandscapeDotsLeft.text = GM.DotsLeft.ToString();							// Переделываем числовое значение в строковое и передаём на экран	для "Landscape" интерфейса
		for (byte a = 0; a < 4; a++)														// 4 раза прогоняем цикл
		{
			PortraitControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true; 	// И ставим все элементы управления пекменом в режим активных для "Portrait" интерфейса
			LandscapeControlOfPacman.transform.GetChild(a).GetComponent<Button>().interactable = true; 	// И ставим все элементы управления пекменом в режим активных для "Landscape" интерфейса
		}
	}

		
	void MethodEndGame()																		// Метод вызываемый событием "Конец игры"
	{
		PortraitGameOver.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = GM.GamePoints.ToString(); // Передаём строке значение общего количества очков пекмена которая отображает их в конце игры для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(3).GetChild(1).GetComponent<Text>().text = GM.GamePoints.ToString(); // Передаём строке значение общего количества очков пекмена которая отображает их в конце игры для "Landscape" интерфейса
		StartCoroutine(EndGamePlus());													// Вызываем Метод-Корутину
	}
	IEnumerator EndGamePlus()																// Этот метод продолжение метода "Конец игры" но способный отсчитывать время
	{
		yield return new WaitForSeconds(3);
		PortraitGameOver.SetActive(true);												// Включаем панель GameOver для "Portrait" интерфейса
		LandscapeGameOver.SetActive(true);												// Включаем панель GameOver для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(1).gameObject.SetActive(false);	// Отключаем панель кнопок управления пекменом для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(1).gameObject.SetActive(false);// Отключаем панель кнопок управления пекменом для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(false);	// Отключаем панель очков игрока для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(false);// Отключаем панель очков игрока для "Landscape" интерфейса

		Image PortraitIm = MainMenu.transform.GetChild(0).GetChild(3).GetChild(2).GetComponent<Image>();	// Достаём компонент Image из изображения YouWin и ложим его в переменную PortraitIm для "Portrait" интерфейса
		Image LandscapeIm = MainMenu.transform.GetChild(1).GetChild(3).GetChild(2).GetComponent<Image>();	// Достаём компонент Image из изображения YouWin и ложим его в переменную LandscapeIm для "Landscape" интерфейса
		float Transparency = 0;																	// Переменная отвечающая за прозрачность изображения
		yield return new WaitForSeconds(1);													// Ждём
		for(; Transparency<1;)																	// Проходим цикл делая текст "GameOver" Видимым
		{
			Transparency += 0.05f;																// Прибавляем прозрачность переменной Transparency
			yield return new WaitForSeconds(0.06f);										// Ждём
			PortraitIm.color = (Color32)new Color(250, 250, 250, Transparency);	// Присваиваиваем новую прозрачность изображению "GameOver" для "Portrait" интерфейса
			LandscapeIm.color = PortraitIm.color;											// Копируем прозрачность изображения "GameOver" c  "Portrait" интерфейса для "Landscape" интерфейса
		}
		yield return new WaitForSeconds(1);													// Ждём
		PortraitGameOver.transform.GetChild(1).gameObject.SetActive(true);		// Включаем панель кнопок новая игра/выход для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(1).gameObject.SetActive(true);		// Включаем панель кнопок новая игра/выход для "Landscape" интерфейса
		PortraitGameOver.transform.GetChild(3).gameObject.SetActive(true); 		// Делаем активной панель показывающую общее количество очков пекмена для "Portrait" интерфейса
		LandscapeGameOver.transform.GetChild(3).gameObject.SetActive(true); 		// Делаем активной панель показывающую общее количество очков пекмена для "Landscape" интерфейса
	}


	void MethodPause()																			// Этот метод подписан на событие "Пауза"
	{
		PortraitInterface.transform.GetChild(5).gameObject.SetActive(true);		// Включаем панель(меню) паузы для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(5).gameObject.SetActive(true);		// Включаем панель(меню) паузы для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(1).gameObject.SetActive(false);		// Отключаем панель кнопок управления пекменом  для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(1).gameObject.SetActive(false);	// Отключаем панель кнопок управления пекменом  для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(false);		// Отключаем панель очков игрока для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(false);	// Отключаем панель очков игрока для "Landscape" интерфейса
		PortraitPauseText.SetActive(true);													// Показываем текст предупреждения выхода из игры для "Portrait" интерфейса
		LandscapePauseText.SetActive(true);													// Показываем текст предупреждения выхода из игры для "Landscape" интерфейса
	}


	void MethodPlay()																				// Этот метод подписан на событие "Снять с паузы"
	{
		PortraitInterface.transform.GetChild(5).gameObject.SetActive(false);		// Отключаем панель(меню) паузы для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(5).gameObject.SetActive(false);	// Отключаем панель(меню) паузыдля "Landscape" интерфейса
		PortraitInterface.transform.GetChild(1).gameObject.SetActive(true);		// Включаем панель кнопок управления пекменом для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(1).gameObject.SetActive(true);		// Включаем панель кнопок управления пекменом для "Landscape" интерфейса
		PortraitInterface.transform.GetChild(2).gameObject.SetActive(true);		// Включаем панель очков игрока для "Portrait" интерфейса
		LandscapeInterface.transform.GetChild(2).gameObject.SetActive(true);		// Включаем панель очков игрока для "Landscape" интерфейса
		PortraitPauseText.SetActive(false);													// Скрываем текст предупреждения выхода из игры для "Portrait" интерфейса
		LandscapePauseText.SetActive(false);												// Скрываем текст предупреждения выхода из игры для "Landscape" интерфейса
	}


	void OnDisable()
	{
		GameManager.StartGame -= MethodStartGame;								// Отписываем метод "MethodStartGame" от события "StartGame" 
		GameManager.ScreenNowPortrait -= MethodScreenNowPortrait;		// Отписываем метод "MethodScreenNowPortrait" от cобытия "ScreenNowPortrait"
		GameManager.ScreenNowLandscape -= MethodScreenNowLandscape;		// Отписываем метод "MethodScreenNowLandscape" от cобытия "ScreenNowLandscape"
		GameManager.KillPacMan -= MethodKillPacMan;							// Отписываем метод "MethodKillPacMan" от события "KillPacMan"
		GameManager.AllDotsAreCollected -= MethodAllDotsAreCollected;	// Отписываем метод "MethodAllDotsAreCollected" от события "Все точки собранны" 
		GameManager.NewGame -= MethodNewGame;									// Отписываем метод "MethodNewGame" от события "NewGame"
		GameManager.Pause -= MethodPause;										// Отписываем метод "MethodPause" от события "Pause"
		GameManager.Play -= MethodPlay;											// Отписываем метод "MethodPlay" от события "Play"
		GameManager.EndGame -= MethodEndGame;									// Отписываем метод "MethodEndGame" от события "EndGame"
	}
}
