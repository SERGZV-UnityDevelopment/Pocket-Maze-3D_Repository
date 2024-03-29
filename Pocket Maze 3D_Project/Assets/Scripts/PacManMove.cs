using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PacManMove : MonoBehaviour
{
	public float Speed;														// Скорость пемена
	public GameObject MainMenu;											// Переменная для главного канваса "MainMenu"
	public GUInterface GUInt;												// Скрипт ГУИ
	GameObject PortraitInterface;											// Панель содержащая в себе весь "Портрет" интерфейс
	Vector3 CurrentVector = Vector3.zero;								// Вектор текущего направления	
	Vector3 PlanVector = Vector3.zero;									// Запланированное направление
	Vector3 DefferedPlanVector;											// Отложенный вектор запланированного направления
	GameManager GM;															// Скрипт гейм менеджера
	Vector3 Dest = Vector3.zero;											// Точка назначения
	Animator PacManAnimator;												// Аниматор Пекмена
	enum SideEnum : byte {Right, Left, Up, Down};					// Перечисление списка вариантов возможных сторон в которую может двигаться пемен


	void OnEnable()
	{
		GameManager.StartLevel += MethodStartLevel;							// Подписываем метод "MethodStartLevel" на событие "StartLevel"
		GameManager.CherryWasTaken += MethodCherryWasTaken;				// Подписываем метод "MethodCherryWasTaken" на событие "CherryWasTaken"
		GameManager.KillPacMan += MethodKillPacMan;							// Подписываем метод "MethodKillPacMan" на событие "KillPacMan"
		GameManager.AllDotsAreCollected += MethodAllDotsAreCollected;	// Подписываем метод "MethodAllDotsAreCollected" на cобытие "Все точки собранны" 
		GameManager.NewGame += MethodNewGame;									// Подписываем метод "MethodNewGame" на событие "NewGame"
	}


	void Start() 						
	{
		Dest = transform.position;
	
		PacManAnimator = GetComponent<Animator>();
	
		GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>(); 			// Находим скрипт гейм менеджера и ложим его в "GM"
	}


	void FixedUpdate() 
	{
		Dirrecting();		// Вызываем метод направления
		Moving ();			// Вызываем метод движения
		Animation();		// Вызываем метод анимации
	}


	public void RightButton()											// Если мы нажали кнопку в право
	{
		DefferedPlanVector = Vector2.right;							// То ставим пекмену запланированное направление вправо	
	}


	public void LeftButton()											// Если мы нажали кнопку в лево
	{
		DefferedPlanVector = Vector2.left;							// То ставим пекмену запланированное направление влево
	}


	public void UpButton()												// Если мы нажали кнопку в верх
	{
		DefferedPlanVector = Vector3.forward;						// То ставим пекмену запланированное направление в верх
	}


	public void DownButton()											// Если мы нажали кнопку в низ
	{
		DefferedPlanVector = Vector3.back;							// То ставим пекмену запланированное направление в низ
	}
		

	void Dirrecting()														// В этом методе мы указываем какое направление запланировать для пекмена
	{
		PlanVector = DefferedPlanVector;								// Присваиваем значение DefferedPlanVector переменной PlanVector
	}


	void Turn()																// Этот метод поворачивает призрака в направлении его движения
	{
		if(CurrentVector == new Vector3(1,0,0))					// В случае если пекмен дивжеться направо
			transform.rotation = Quaternion.Euler(0, 90, 0);	// Поворачиваем его на право
		else if(CurrentVector == new Vector3(-1,0,0))			// В случае если пекмен движеться на лево
			transform.rotation = Quaternion.Euler(0, -90, 0);	// Поворачиваем его на лево
		else if(CurrentVector == new Vector3(0,0,1))				// В случае если пекмен движеться вперёд
			transform.rotation = Quaternion.Euler(0, 0, 0);		// Поворачиваем его лицом вперёд
		else if(CurrentVector == new Vector3(0,0,-1))			// В случае если пекмен движеться назад
			transform.rotation = Quaternion.Euler (0, 180, 0);	// Разврачиваем его лицом назад
	}


	void Moving()
	{	
		Vector3 P = Vector3.MoveTowards(transform.position, Dest, (Speed/100 * GM.GameLevel));	// Высчитываем вектор движения пекмена, скорость определяеться по формуле (Speed/100 * "Номер уровня")
		GetComponent<Rigidbody>().MovePosition(P);										// Двигаемся к месту назначения используя полученный выше вектор


 		if(transform.position == Dest)														// Если пекмен достиг точки назначения
		{
			if(CurrentVector != Vector3.zero || PlanVector != Vector3.zero)		// И есть запланированное направление или есть текущее направление
			{
				if(PlanVector != CurrentVector && ValidPlan(PlanVector))				// Если вектор запланированного направления не равен вектору текущего направления и завернуть в запланированном направлении возможно
				{
					Dest = (transform.position + PlanVector);								// То конвертируем направление вектора в новую точку назначения пекмена
					CurrentVector = PlanVector;												// А так же ставим планируемое направление текущим направлением
				}
				else 																					// Если не возможно завернуть в запланированном направлении мы спрашиваем
				{
					if(Valid())																		// Если возможно продолжать движение в текущем направлении
						Dest = transform.position + CurrentVector; 						// То присваиваем переменной Dest следующую точку в том же направлении
					else 																				// Иначе если не возможно
					{
						Dest = transform.position;												// То ставим направление нулевым
						CurrentVector = Vector3.zero;											// Обнуляем вектор направления
						PlanVector = Vector3.zero;												// Обнуляем вектор планируемого направления так как завернуть мы не смогли а по этому направлению мы зашли в тупик
						DefferedPlanVector = Vector3.zero;									// И обнуляем отложенный вектор запланированного обновления
					}
				}
			}
			Turn();																					// Вызываем метод поворота
		}
	}


	bool ValidPlan(Vector3 PlVec)																								// Метод опрашивающий возможен ли поворот в запланированном направлении
	{
		RaycastHit Hit;																											// Сюда подаёться информация о коллайдере в который врезалься луч
		Vector3 Pos = (transform.position + new Vector3(0f ,0.5f ,0f));											// Переменная Pos сюда ложиться текущая позиция "Пекмена" + высота до центра пекмена

		if(Physics.SphereCast(Pos, 0.2f, new Vector3(PlVec.x, PlVec.y, PlVec.z), out Hit, 1.3f, 1<<8)) 	// Если луч столкнёться с коллайдером лабиринта значит повернуть не возможно (10 перегрузка метода)
			return false;																											// И метод возвращает правда
		else 																															// Иначе если луч не ударилься о коллайдер лабиринта значит поворот всётаки возможен
			return true;																											// Иначе метод возвращает ложь
	}


	bool Valid()
	{
		// Прочертим линию на один юнит выше пекмена и посмотрим удариться ли он обо что не будь
		Vector3 RayPos = transform.position + new Vector3(0f ,0.5f ,0f);											// Переменная RayPos сюда ложиться текущая позиция "Пекмена" + высота до центра пекмена
		Vector3 RayDest = ((Dest + new Vector3(0f ,0.5f ,0f)) + (CurrentVector * 1.1f));						// Переменная RayDest сюда ложиться позиция места куда мы будем стрелять лучом проверки наличия коллайдера

		if(Physics.Linecast(RayPos, RayDest, 1<<8))			// Если луч врезалься в коллайдер лабиринта значит дальше движение продолжать не возможно	
			return false;												// Метод возвращает Ложь
		else																// Иначе если луч не нашёл коллайдер
			return true;												// Метод возвращает правда
	}


	void Animation ()													// Этот метод задаёт анимацию пекмену
	{
		if(CurrentVector == Vector3.zero)						// Если переменная PlanVector равна нулю
			PacManAnimator.SetBool("Moving", false);			// Указываем переменной аниматора Пекмена "Moving" значение ложь
		else 
			PacManAnimator.SetBool("Moving", true);			// Указываем переменной аниматора Пекмена "Moving" значение правда
	}


	void OnTriggerEnter(Collider Co)														// Этот метод вызываеться при вхождении какого либо коллайдера в коллайдер пекмена
	{
		if(Co.tag == "PacDot")																// Если тег объекта этого коллайдера "PacDot"
		{
			GM.DotsLeft --;																	// Отнимаем количество дотов которые осталось собрать
			GM.DotsEaten ++;																	// Прибавляем количество съеденных дотов
			GM.GamePoints ++;																	// Прибавляем одно очко
			UpdatePoints();																	// Вызываем метод обновляющий количество пекдотов и очков
			Destroy(Co.gameObject);															// Уничтожаем конкретно этот "PacDot"
		}
		if(Co.tag == "Ghost") 																// Если тег коллайдера призрак
		{
			if(GM.PacManDanger == false)													// Если пекмен не опасен
			{
				GM.KillPacManStarter();														// Вызываем метод вызывающий событие убить пекмена
			}
			else 																					// Иначе если пекмен опасен
			{
				if(GM.PacmanLifes < 2)														// Если у пекмена меньше двух жизней
				{
					GUInt.PortraitPacManLifes.transform.GetChild(GM.PacmanLifes).gameObject.GetComponent<Image>().sprite = GUInt.PacManIcons[1];	// Добавляем один значёк жизней пекмена для "Portrait" интерфейса
					GUInt.LandscapePacManLifes.transform.GetChild(GM.PacmanLifes).gameObject.GetComponent<Image>().sprite = GUInt.PacManIcons[1];	// Добавляем один значёк жизней пекмена для "Landscape" интерфейса
					GM.PacmanLifes++;															// Прибавляем одну жизнь пекмену
				}
				GM.GamePoints += 5;															// Прибавляем пять очков пекмену
				UpdatePoints();																// Вызываем метод обновляющий количество пекдотов и очков
			}
		}
		if(Co.tag == "Cherry")												// Если тег объекта этого коллайдера "Cherry"
		{
			GM.CherryWasTakenStarter();									// Вызываем метод вызывающий событие "Вишня была взята"
			GM.DotsLeft --;													// Отнимаем количество дотов которые осталось собрать
			GM.DotsEaten ++;													// Прибавляем количество съеденных дотов
			GM.GamePoints ++;													// Прибавляем 1 очко за съеденную вишню
			UpdatePoints();													// Вызываем метод обновляющий количество пекдотов и очков
			Destroy(Co.gameObject);											// Уничтожаем вишню
		}
		if(Co.tag == "Deceleration")										// Если тег объекта этого коллайдера "Deceleration"
		{
			GM.DecelerationWasTakenStarter();							// Вызываем метод вызывающий событие "Замедление было взято"
			GM.DotsLeft --;													// Отнимаем количество дотов которые осталось собрать
			GM.DotsEaten ++;													// Прибавляем количество съеденных дотов
			GM.GamePoints ++;													// Прибавляем 1 очко за съеденное замедление
			UpdatePoints();													// Вызываем метод обновляющий количество пекдотов и очков
			Destroy(Co.gameObject);											// Уничтожаем замедление
		}
		if(Co.tag == "Freezing")											// Если тег объекта этого коллайдера "Freezing"
		{
			GM.FreezingWasTakenStarter();									// Вызываем метод вызывающий событие "Заморозка была взята"
			GM.DotsLeft --;													// Отнимаем количество дотов которые осталось собрать
			GM.DotsEaten ++;													// Прибавляем количество съеденных дотов
			GM.GamePoints ++;													// Прибавляем 1 очко за съеденную заморозку
			UpdatePoints();													// Вызываем метод обновляющий количество пекдотов и очков
			Destroy(Co.gameObject);											// Уничтожаем заморозку
		}
		if(Co.tag == "GateTrigger")										// Если тег объекта этого коллайдера "Ворота"
		{
			if(Co.transform.parent.name == "OrangeGate (1)")		// Если это ворота номер 1
			{
				transform.position = new Vector3(-0.15f, 0, 18);	// Телепортируем пекмена ко вторым воротам
				Dest = new Vector3( 2, 0, 18);							// Ставим ему новую точку назначения
			}
			else if(Co.transform.parent.name == "BlueGate (2)") 	// Иначе если это ворота номер 2
			{
				transform.position = new Vector3(31.15f, 0, 18);	// Телепортируем пекмена ко вторым воротам
				Dest = new Vector3( 29, 0, 18);							// Ставим ему новую точку назначения
			}
		}
	}


	void UpdatePoints()																	// Этот метод обновляет количество очков и дотов на экране
	{
		GUInt.PortraitDotsLeftT.text = GM.DotsLeft.ToString();				// Переделываем числовое значение оставшихся дотов в строковое и передаём на экран для "Portrait" интерфейса
		GUInt.LandscapeDotsLeft.text = GM.DotsLeft.ToString();				// Переделываем числовое значение оставшихся дотов в строковое и передаём на экран для "Landscape" интерфейса
		GUInt.PortraitGamePoints.text = GM.GamePoints.ToString();			// Переделываем числовое значение очков пекмена в строковое и передаём на экран для "Portrait" интерфейса
		GUInt.LandscapeGamePoints.text = GM.GamePoints.ToString();			// Переделываем числовое значение очков пекмена в строковое и передаём на экран для "Landscape" интерфейса
	}
	//------------------------------------------------------------------------------------------------- События ---------------------------------------------------------------------------------------------------------------------------------------------


	void MethodStartLevel()																	// Этот метод подписан на событие "Старт уровня"
	{
		PacManAnimator.SetFloat("SpeedMultiplier", (GM.GameLevel/10f)+0.9f);	// Указываем уровень игры в качестве множителя для скорости
	}


	void MethodCherryWasTaken()															// Метод вызванный событием вишня была взята
	{
		
	}


	void MethodKillPacMan()																		// Метод вызванный событием "Убить пекмена"	
	{
		Dest = new Vector3 (15, -3, 15);														// Ставим точку назначения пекмена под лабиринт
		transform.position = Dest;																// Убираем пекмена на точку его назначения
		CurrentVector = Vector3.zero;															// Обнуляем вектор направления
		PlanVector = Vector3.zero;																// Обнуляем вектор планируемого направления 
		DefferedPlanVector = Vector3.zero;													// И обнуляем отложенный вектор запланированного обновления
		StartCoroutine(IEnumKillPacMan());													// Вызываем корутину "IEnumKillPacMan"
	}
	IEnumerator IEnumKillPacMan()																// Этот метод-корутина продолжение метода "MethodKillPacMan"
	{
		yield return new WaitForSeconds(2);													// Ждём 2 секунды	
		transform.position = new Vector3 (15, 0, 15);									// Ставим пекмена на место
		transform.rotation = Quaternion.Euler (0, 90, 0);								// Ставим начальное вращение пекмена
		Dest = transform.position;																// Ставим точку назначения пекмена в него самого
	}


	void MethodAllDotsAreCollected ()											// Этот метод запускает корутину
	{
		StartCoroutine(IEnumAllDotsAreCollected());							// Вызываем Метод способный отсчитывать время	
	}
	IEnumerator IEnumAllDotsAreCollected()										// Корутина все доты были собранны
	{
		CurrentVector = Vector3.zero;												// Обнуляем вектор направления
		PlanVector = Vector3.zero;													// Обнуляем вектор планируемого направления 
		DefferedPlanVector = Vector3.zero;										// И обнуляем отложенный вектор запланированного направления
		Dest = transform.position;													// Ставим точку назначения пекмена на его текущее местоположение

		if(GM.GameLevel <= 10)
		{
			yield return new WaitForSeconds(1.5f);									// Ждём
			Dest = new Vector3 (15, 0, 15);											// Ставим точку назначения пекмена на стартовое место
			transform.position = Dest;													// Убираем пекмена на точку его назначения
			transform.rotation = Quaternion.Euler (0, 90, 0);					// Ставим начальное вращение пекмена
		}
		else
		{
			yield return new WaitForSeconds(3.2f);									// Ждём
			Dest = new Vector3 (15, 0, 15);											// Ставим точку назначения пекмена на стартовое место
			transform.position = Dest;													// Убираем пекмена на точку его назначения
			transform.rotation = Quaternion.Euler (0, 90, 0);					// Ставим начальное вращение пекмена
		}
	
	}


	void MethodNewGame()																// Этот метод вызываеться событием "Новая игра"
	{
		
	}


	void OnDisable()
	{
		GameManager.StartLevel -= MethodStartLevel;							// Отписываем метод "MethodStartLevel" от события "StartLevel"
		GameManager.CherryWasTaken -= MethodCherryWasTaken;				// Отписываем метод "MethodCherryWasTaken" от события "CherryWasTaken"
		GameManager.KillPacMan -= MethodKillPacMan;							// Отписываем метод "MethodKillPacMan" от события "KillPacMan"
		GameManager.AllDotsAreCollected -= MethodAllDotsAreCollected;	// Отписываем метод "MethodAllDotsAreCollected" от события "Все точки собранны" 
		GameManager.NewGame -= MethodNewGame;									// Отписываем метод "MethodNewGame" от события "NewGame"
	}


}
