using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostMove : MonoBehaviour 
{
	public enum GhostN : byte {Hunter, Ambusher, Shy, HomeBody}										// Перечисление приведений (Охотник, Сидящий в засаде, Стеснительный, Домосед)
	public GhostN GhostName;																					// Объект перечисления типа призраков
	public enum GhostM : byte {Expectation, LeavingTheHouse, Pursuit, RunUp, fright}			// Перечисление режимов приведения, (1-режим ожидания, 2-Режим выхода из дома, 3-режим преследования, 4-режим разбегания, и 5-режим испуга)
	public GhostM GhostMode;																					// Объект перечисления режима призраков
	public float Speed;																							// Скорость призрака
	public bool Home;																								// Эта переменная указывает дома ли находиться призрак
	public short NomberDotsForActivation;																	// Сколько дотов пекмену надо собрать чтобы активировать этого призрака
	public Vector3 DestinationPoint;																			// Точка назначения
	public float Delay;																							// Задержка для поочерёдного выхода приведений из домика
	public float DelayTimer;																					// Таймер задержки для поочерёдного выхода приведений из домика
	public Material Original;																					// Оригинальный материал этого приведения		
	public Vector3 RunUpPoint;																					// Точка куда будет убегать приведение в режиме разбегания
	float SpeedModifier = 0;																					// Модификатор скорости приведения, нужен чтобы уменьшать его скорость при замедлении или совсем остановить при заморозке																
	Vector3 StartPosition;																						// Стартовая позиция
   GameManager GM;                                                             				// Скрипт Гейм менеджера
	Vector3 TheLastPosition;																					// Сдесь будет находиться последня позиция в которой был пекмен
	Vector3 GhostStartPosition = new Vector3(15, 0f, 21f);											// Стартовая позиция с которой вступают в игру призраки			
	GameObject PacMan;																							// Тут будет лежать объект пемен
	Rigidbody GhostRB;																							// Сдесь будет храниться Rigitbody призрака
	Vector3 Pos;																									// Переменная Pos сюда ложиться текущая позиция призрака + высота в 0.5 метров нужная для 
	Ray[] DirrectionRays = new Ray[4];																		// Массив 4 потенциальных направлений выстрела луча для проверки возможности движения
	Vector3[] PotencialPositions = new Vector3[4];														// Массив позиций которые призрак обнаружил как потенциальные направления


	void OnEnable()																								// При активации скрипта
	{
		GameManager.StartGame += MethodStartGame;															// Подписываем метод "MethodStartGame" на cобытие "StartGame" 
		GameManager.ToPursuit += MethodToPursuit;															// Подписываем метод "MethodToPursuit" на cобытие "ToPursuit" 
		GameManager.ToRunUp += MethodToRunUp;																// Подписываем метод "MethodToRunUp" на cобытие "ToRunUp" 
		GameManager.CherryWasTaken += MethodCherryWasTaken;											// Подписываем метод "MethodCherryWasTaken" на событие "CherryWasTaken"
		GameManager.DecelerationWasTaken += MethodDecelerationWasTaken;							// Подписываем метод "MethodDecelerationWasTaken" на событие "DecelerationWasTaken"
		GameManager.FreezingWasTaken += MethodFreezingWasTaken;										// Подписываем метод "MethodFreezingWasTaken" на событие "FreezingWasTaken"
		GameManager.GhostReturnNormalSpeed += MethodGhostReturnNormalSpeed;						// Подписываем метод "MethodGhostReturnNormalSpeed" на событие "GhostReturnNormalSpeed"
		GameManager.PacManNotDanger += MethodPacManNotDanger;											// Подписываем метод "PacManNotDanger" на событие "MethodPacManNotDanger"
		GameManager.KillPacMan += MethodKillPacMan;														// Подписываем метод "MethodKillPacMan" на событие "KillPacMan"
		GameManager.AllDotsAreCollected += MethodAllDotsAreCollected;								// Подписываем метод "MethodAllDotsAreCollected" на cобытие "Все точки собранны" 
		GameManager.NewGame += MethodNewGame;																// Подписываем метод "MethodNewGame" на событие "NewGame"
		GameManager.EndGame += MethodEndGame;																// подписываем метод "MethodEndGame" на событие "EndGame"
	}


	void Start() 
	{
		StartPosition = transform.position;
		DestinationPoint = transform.position;																// Задаём начальные данные переменной "DestinationPoint"
		PacMan = GameObject.FindGameObjectWithTag("PacMan");											// Ищем на сцене пекмена и ложим его в GameObject пекмен
		GhostRB = gameObject.GetComponent<Rigidbody>();													// Ищем на призраке компонент RidgitBody и ложим его в переменую GhostRB
		GhostMode = GhostM.Expectation;																		// Выставляем режим по умолчанию на ожидание
		GM = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();	// Находим скрипт гейм менеджера и ложим его в "GM"
		DirrectionRays[0] = new Ray(Pos, new Vector3(0, 0, 1));										// Заносим в массив направление луча вперёд
		DirrectionRays[1] = new Ray(Pos, new Vector3(1, 0, 0));										// Заносим в массив направление луча вправо
		DirrectionRays[2] = new Ray(Pos, new Vector3(0, 0, -1));										// Заносим в массив направление луча вниз
		DirrectionRays[3] = new Ray(Pos, new Vector3(-1, 0, 0));										// Заносим в массив направление луча влево
	}


	void Update()															// Вызываеться на каждый кадр
	{
		TimerDelay();														// Вызываем метод таймер отсчитывающий время задержки для поочерёдного выхода приведений из домика
	}


	void FixedUpdate() 													// Вызываеться фиксированное количество раз в секунду указанное в настройках игры
	{	
		GhostModeSelection();											// Вызываем метод выбирающий режим приведения
		Moving();															// Двигаем приведение вперёд
	}


	void GhostModeSelection()											// Этот метод отвечает за выбор режима приведения				
	{
		if(GM.PlayGame == true) 										// Если режим игры активирован
		{
			if(DelayTimer <= 0) 											// Если задержка равна нулю
			{
				if(transform.position == DestinationPoint)		// Если мы достигли точки назначения (Это нужно чтобы призрак не задумывалься очень часто что ему делать дальше)
				{
					switch(GhostMode)										// Опрашиваем режим приведения для решения поведения
					{
						case GhostM.Expectation:						// Если режим приведения "Ожидание"
						{ 							
							Expectation();									// Вызываем метод ожидания
						}
						break;
						case GhostM.LeavingTheHouse:					// Если режим приведения "Выход из дома"
						{
							LeavingTheHouse();							// Вызываем метод "Покинуть дом"
						}
						break;
						case GhostM.Pursuit:								// Если режим приведения "Погоня"
						{
							Pursuit();										// То вызываем метод преследования
						}
						break;
						case GhostM.RunUp:								// Если режим приведения "Разбегание" 
						{
							RunUp();											// То вызываем метод "Разбегание"
						}
						break;
        	  	 		case GhostM.fright:								// Если режим приведения "Испуг"
           	 		{
							Fright();										// Вызываем метод испуга
           			}
           	 		break;
					}
				}
			}
		}
	}


	void Expectation() 																	// Этот метод отвечает за режим ожидания
	{
		if(GM.DotsEaten >= NomberDotsForActivation)								// Если количество дотов которое нужно для активации это приведения больше или равно
		{		
			if(Home)																			// Если призрак дома
				GhostMode = GhostM.LeavingTheHouse;									// То включаем режим покинуть дом
			else 																				// Иначе если призрак уже покинул дом
			{
				if(GM.ActiveModeOfGhosts != GameManager.ActModOfG.Fright)	// Если пекмен в данный момент не опасен
					GhostMode = GhostM.Pursuit;										// То переключаем режим приведения на погоню	
				else 																			// Иначе если он опасен
					GhostMode = GhostM.fright;											// Включаем режим страха
			}
		}
	}


	void LeavingTheHouse()																// Этот метод отвечает за то чтобы каждое приведение смогло покинуть свой дом для вступления в игру
	{
		if(transform.position != GhostStartPosition)								// Если призрак ещё не на своей стартовой позиции
		{
			TheLastPosition = transform.position;									// Записываем его реальную позицию как старую
	
			if(DestinationPoint != new Vector3(15, 0, 18))						// И если призрак не стоит на прямой линии выхода из дома 
				DestinationPoint = new Vector3(15, 0, 18);						// То ставим ему это местоположение как отправную точку
			else 																				// Иначе если он уже стоит на этой точке
				DestinationPoint = GhostStartPosition;								// Устанавливаем ему точку назначения для выхода из дома	
		}
		else 																					// Иначе если призрак оказалься на своей стартовой позиции
		{
			Home = false;																	// Указываем что он вышел из дома
																								// И выбираем какой режим ему нужно сейчас использовать преследование, страх или разбегание
			if(GM.ActiveModeOfGhosts == GameManager.ActModOfG.Pursuit)		// Если пекмен не опасен и общий для всех приведений режим это "Преследование"
				GhostMode = GhostM.Pursuit;											// То режим приведения переключаем на "Преследование"
			else if(GM.ActiveModeOfGhosts == GameManager.ActModOfG.Fright)	// Если общий для всех приведений режим это "Страх"
				GhostMode = GhostM.fright;												// Включаем призраку режим страха	
			else if(GM.ActiveModeOfGhosts == GameManager.ActModOfG.RunUp)	// Если общий для всех приведений режим это "Разбегание"
				GhostMode = GhostM.RunUp;												// Включаем призраку режим разбегания					
		}		
		Turn();																				// Вызываем метод поворота
	}


	void Pursuit()																				// В режиме преследования приведения преследуют пекмена с целью съесть его
	{
		Pos = (transform.position + new Vector3 (0f, 0.5f, 0f));					// Обновляем переменную Pos
		Vector3 SelectedPoint = new Vector3(0, 0, 0);								// Выбранная для перемещения точка к которой будет двигаться приведение
		float DistanceToPacman = 100;														// Здесь храниться наименьшая дистанция до пекмена
		RaycastHit Hit;																		// Переменная куда возвращаються данные об ударе луча
		byte b = 0;																				// Вторая переменная счётчика цикла считает только свободные точки (Потенциальные направления)

		for(byte a=0; a<4; a++) 															// Заканчиваем цикл после прошествия 4 итерации
		{
			DirrectionRays[a].origin = Pos;												// Обновляем позицию луча откуда будем стрелять
			Physics.SphereCast(DirrectionRays[a], 0.2f, out Hit, 1.3f, 1<<8);	// Пускаем луч в этом цикле в указанную в массиве сторону	
			if(Hit.collider == null)														// Если коллайдер лабиринта не был обнаружен в этом направлении
			{
				PotencialPositions[b] = new Ray(transform.position, DirrectionRays[a].direction).GetPoint(1); 	// Заносим точку в этом направлении как потенциальную	
				SelectedPoint = PotencialPositions[b];									// Ложим в переменную выбранная точка, последнюю найденную потценциальную позицию
				b++;																				// Добавляем к переменной b еденицу
			}
		}
		if(b >= 2) 																				// Если было обнаруженно две или более доступных точки
		{
			for(byte c=0; c<b; c++)															// То отсеиваем ту на которой призрак был до этого а оставшуюся/оставшиеся сравниваем по длинне луча и выбираем с самым коротким до пекмена
			{
				if(PotencialPositions[c] != TheLastPosition)							// Если потенциальная позиция и предыдущая позиция разные		
				{	// То сравниваем дистанцию от этой точки до пекмена с предыдущим рекордсменом на короткое расстояние "DistanceToPacman" и если новое расстояние оказалось короче
					if(Vector3.Distance(PotencialPositions[c], PacMan.transform.position) < DistanceToPacman)	
					{
						DistanceToPacman = Vector3.Distance(PotencialPositions[c], PacMan.transform.position);	// Заносим это расстояние как рекордсмен в переменную "DistanceToPacman"
						SelectedPoint = PotencialPositions[c];																	// Заносим новую рекордную позицию в переменную SelectedPoint
					}
				}
			}
		}
		TheLastPosition = transform.position;											// Записываем его реальную позицию как старую
		DestinationPoint = SelectedPoint;												// Ставим выбранную для движения точку как точку назначения
		Turn();																					// Вызываем метод поворота
	}


	void RunUp() 																				// Режим разбегания приведений по разным углам
	{
		Pos = (transform.position + new Vector3 (0f, 0.5f, 0f));					// Обновляем переменную Pos
		Vector3 SelectedPoint = new Vector3(0, 0, 0);								// Выбранная для перемещения точка к которой будет двигаться приведение
		float DistanceToRunUpPoint = 100;												// Здесь храниться наименьшая дистанция до точки разбегания
		RaycastHit Hit;																		// Переменная куда возвращаються данные об ударе луча
		byte b = 0;																				// Вторая переменная счётчика цикла считает только свободные точки (Потенциальные направления)

		for(byte a=0; a<4; a++) 															// Заканчиваем цикл после прошествия 4 итерации
		{
			DirrectionRays[a].origin = Pos;												// Обновляем позицию луча откуда будем стрелять
			Physics.SphereCast(DirrectionRays[a], 0.2f, out Hit, 1.3f, 1<<8);	// Пускаем луч в этом цикле в указанную в массиве сторону	
			if(Hit.collider == null)														// Если коллайдер лабиринта не был обнаружен в этом направлении
			{
				PotencialPositions[b] = new Ray(transform.position, DirrectionRays[a].direction).GetPoint(1); 	// Заносим точку в этом направлении как потенциальную	
				SelectedPoint = PotencialPositions[b];									// Ложим в переменную выбранная точка, последнюю найденную потценциальную позицию
				b++;																				// Добавляем к переменной b еденицу
			}
		}
		if(b >= 2) 																				// Если было обнаруженно две или более доступных точки
		{
			for(byte c=0; c<b; c++)															// То отсеиваем ту на которой призрак был до этого а оставшуюся/оставшиеся сравниваем по длинне луча и выбираем с самым коротким до точки убегания
			{
				if(PotencialPositions[c] != TheLastPosition)							// Если потенциальная позиция и предыдущая позиция разные		
				{	// То сравниваем дистанцию от этой точки до точки разбегания с предыдущим рекордсменом на короткое расстояние "DistanceToRunUpPoint" и если новое расстояние оказалось короче
					if(Vector3.Distance(PotencialPositions[c], RunUpPoint) < DistanceToRunUpPoint)	
					{
						DistanceToRunUpPoint = Vector3.Distance(PotencialPositions[c], RunUpPoint);				// Заносим это расстояние как рекордсмен в переменную "DistanceToRunUpPoint"
						SelectedPoint = PotencialPositions[c];																	// Заносим новую рекордную позицию в переменную SelectedPoint
					}
				}
			}
		}
		TheLastPosition = transform.position;																// Записываем его реальную позицию как старую
		DestinationPoint = SelectedPoint;																	// Ставим выбранную для движения точку как точку назначения
		Turn();																										// Вызываем метод поворота
	}


	void Fright()																									// Вызываеться когда у приведения активирован режим страха
	{
		Pos = (transform.position + new Vector3 (0f, 0.5f, 0f));										// Обновляем переменную Pos
		Vector3 SelectedPoint = new Vector3(0, 0, 0);													// Выбранная для перемещения точка к которой будет двигаться приведение
		float DistanceToPacman = 0;																			// Здесь храниться наибольшая дистанция до пекмена
		RaycastHit Hit;																							// Переменная куда возвращаються данные об ударе луча
		RaycastHit PacManHit;																					// Эта переменная будет проверяться врезалься ли луч в лабиринт или в пекмена
		byte b = 0;																									// Вторая переменная счётчика цикла считает только свободные точки (Потенциальные направления)

		for(byte a=0; a<4; a++)																					// Стреляем лучом в 4 стороны с длинной луча в один куб
		{
			DirrectionRays[a].origin = Pos;																	// Обновляем позицию луча откуда будем стрелять
			Physics.SphereCast(DirrectionRays[a], 0.2f, out Hit, 1.3f, 1<<8);						// Пускаем луч в этом цикле в указанную в массиве сторону	
			Physics.SphereCast(DirrectionRays[a], 0.2f, out PacManHit, 31, 1<<8 | 1<<10);		// Пускаем луч в этом цикле в указанную в массиве сторону

			if(PacManHit.collider != null)																	// Если длинный луч не ушёл в пустоту за врата
			{
				if(Hit.collider == null && PacManHit.collider.tag != "PacMan")						// Если в переменной Hit не был обнаружен коллайдер а в переменной PacManHit тег коллайдера не равен "PacMan"
				{
					PotencialPositions[b] = new Ray(transform.position, DirrectionRays[a].direction).GetPoint(1);	// Заносим точку в этом направлении как потенциальную	
					SelectedPoint = PotencialPositions[b];													// Ложим в переменную выбранная точка, последнюю найденную потценциальную позицию
					b++;																								// Добавляем к переменной b еденицу
				}
			}
			else 																										// Иначе если ушёл
			{																											// То эта точка подойдёт для побега через врата
				PotencialPositions[b] = new Ray(transform.position, DirrectionRays[a].direction).GetPoint(1);	// Заносим точку в этом направлении как потенциальную	
				SelectedPoint = PotencialPositions[b];														// Ложим в переменную выбранная точка, последнюю найденную потценциальную позицию
				b++;																									// Добавляем к переменной b еденицу
			}
		}
		if(b >= 2) 																									// Если было обнаруженно две или более доступных точки
		{
			for(byte c=0; c<b; c++)																				// То отсеиваем ту на которой призрак был до этого а оставшуюся/оставшиеся сравниваем по длинне луча и выбираем с самым коротким до пекмена
			{
				if(PotencialPositions[c] != TheLastPosition)												// Если потенциальная позиция и предыдущая позиция разные		
				{	// То сравниваем дистанцию от этой точки до пекмена с предыдущим рекордсменом на динное расстояние "DistanceToPacman" и если новое расстояние оказалось длиннее
					if(Vector3.Distance(PotencialPositions[c], PacMan.transform.position) > DistanceToPacman)
					{
						DistanceToPacman = Vector3.Distance(PotencialPositions[c], PacMan.transform.position);	// Заносим это расстояние как рекордсмен в переменную "DistanceToPacman"
						SelectedPoint = PotencialPositions[c];																	// Заносим новую рекордную позицию в переменную SelectedPoint
					}
				}
			}
		}
		else if(b == 0 && SelectedPoint == new Vector3(0, 0, 0))										// Иначе если точка всего лишь одна и та не была выбранна так как приведение зажато между стеной и пекменом
		{
			SelectedPoint = TheLastPosition;																	// Вручную выбираем старую позицию для переменной "выбранная позиция"
		}
		TheLastPosition = transform.position;																// Записываем его реальную позицию как старую
		DestinationPoint = SelectedPoint;																	// Ставим выбранную для движения точку как точку назначения
		Turn();																										// Вызываем метод поворота
	}


	void Turn()																										// Этот метод поворачивает призрака в направлении его движения
	{
		Vector3 NormilizeDir = Vector3.Normalize((DestinationPoint - TheLastPosition));		// Выясняем куда движеться призрак, нормализуем этот вектор и заносим его в переменную NormilizeDir

		if(NormilizeDir == new Vector3(1,0,0))							// В случае если призрак дивжеться направо
			transform.rotation = Quaternion.Euler(0, 90, 0);		// Поворачиваем его на право
		else if(NormilizeDir == new Vector3(-1,0,0))					// В случае если призрак движеться на лево
			transform.rotation = Quaternion.Euler(0, -90, 0);		// Поворачиваем его на лево
		else if(NormilizeDir == new Vector3(0,0,1))					// В случае если призрак движеться вперёд
			transform.rotation = Quaternion.Euler(0, 0, 0);			// Поворачиваем его лицом вперёд
		else if(NormilizeDir == new Vector3(0,0,-1))					// В случае если призрак движеться назад
			transform.rotation = Quaternion.Euler (0, 180, 0);		// Разврачиваем его лицом назад
	}


	void Moving()																																			// Этот метод двигает приведение в указанную точку с указанной скоростью
	{
		Vector3 M = Vector3.MoveTowards(transform.position, DestinationPoint, ((Speed/100*GM.GameLevel)-SpeedModifier));	// Высчитываем вектор движения приведения, скорость определяеться по формуле ((Speed/100 * "Номер уровня") - модификатор скорости)
		GhostRB.MovePosition(M);																														// Заставляем Rigitbody призрака двигаться к новой точке
	}
		

	void TimerDelay()																							// Альтернативный таймер основанный на методе Update
	{
		if(DelayTimer > 0)
			DelayTimer -= (1 * Time.deltaTime);
	}





	void OnTriggerEnter(Collider Co)																		// Этот метод вызываеться при вхождении какого либо коллайдера в коллайдер данного призрака
	{
		if(Co.tag == "PacMan" && GM.ActiveModeOfGhosts == GameManager.ActModOfG.Fright)	// Если тег объекта этого коллайдера "PacMan" и он опасен
		{
			StartCoroutine(DelayGhostRevitalizing());													// Вызываем корутину которая занимаеться уничтожением и оживлением приведения																							
		}
		if(Co.tag == "GateTrigger")																		// Если тег объекта этого коллайдера "Ворота"
		{
			if(Co.transform.parent.name == "OrangeGate (1)")										// Если это ворота номер 1
			{
				transform.position = new Vector3(-0.15f, 0, 18);									// Телепортируем пекмена ко вторым воротам
				DestinationPoint = new Vector3( 2, 0, 18);											// Ставим ему новую точку назначения
			}
			else if(Co.transform.parent.name == "BlueGate (2)") 									// Иначе если это ворота номер 2
			{
				
			}
		}
	}
	IEnumerator DelayGhostRevitalizing()												// Эта корутина занимаеться уничтожением и оживлением приведения		
	{
		GetComponent<CapsuleCollider>().enabled = false;							// Выключаем коллайдер призрака
		transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;	// Выключаем отображение меша глаз приведения
		transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;	// Выключаем отображение меша приведения			
		GhostMode = GhostM.Expectation;													// Ставим приведение в режим ожидания
		transform.position = StartPosition; 											// Телепортируем его в дом на его начальную позицию
		DestinationPoint = StartPosition;												// Ставим стартовую позицию как точку назначения
		transform.rotation = Quaternion.Euler(Vector3.zero);						// Поворачиваем его лицом вперёд	
		if(GhostName != GhostN.Hunter) 													// Если это не призрак охотник 
		{
			Home = true;																		// Указываем что он дома
			DelayTimer = 8;																	// То ставим ему задержку для остальных приведений
		}
		else DelayTimer = 4;																	// Иначе если это охотник ставим задержку для охотника
		yield return new WaitForSeconds(3f);											// Ждём
		GetComponent<CapsuleCollider>().enabled = true;								// Включаем коллайдер призрака
		transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;	// Включаем отображение меша глаз приведения
		transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;	// Выключаем отображение меша приведения	
	}


	//---------------------------------------------------------------------------------------------------------------- События -----------------------------------------------------------------------------------------------------------------------------------


	void MethodStartGame()															// Метод подписанный на событие старта игры
	{

	}


	void MethodToPursuit()															// Метод подписанный на событие переключения в режим преследования
	{
		if(!Home)																		// Если призрак не находиться дома
			GhostMode = GhostM.Pursuit;											// Переключаем его в режим "Погони"
	}


	void MethodToRunUp()																// Метод подписанный на событие переключения в режим разбегания
	{
		if(!Home)																		// Если призрак не находиться дома
			GhostMode = GhostM.RunUp;												// Переключаем его в режим "Разбегания"
	}


	void MethodCherryWasTaken()													// Метод вызванный событием "Вишня была взята"
	{
		if(Home == false)																// Если данное приведение не находиться дома
			GhostMode = GhostM.fright;												// Переключаем режим этого приведения в режим испуга
		gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[1];	// Делаем глаза этого призрака цветом указывающим что его можно съесть
		gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[1];	// Делаем этого призрака цветом указывающим что его можно съесть
	}


	void MethodPacManNotDanger()													// Этот метод вызываеться событием "Пекмен больше не опасен"
	{
		GhostMode = GhostM.Expectation;											// Ставим приведениям режим ожидания
		gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[0];	// Делаем глаза этого призрака цветом указывающим что его можно съесть
		gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material = Original;			// Делаем этого призрака цветом указывающим что его можно съесть
	}


	void MethodDecelerationWasTaken()											// Метод вызванный событием "Замедление было взято"
	{
		gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[2];	// Делаем глаза этого призрака цветом указывающим что он в режиме замедления
		gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[2];	// Делаем этого призрака цветом указывающим что он в режиме замедления
		SpeedModifier = ((Speed/100*GM.GameLevel)/2);						// Устанавливаем модификатор скорости равным половине стандартной скорости этого приведения для этого уровня
	}


	void MethodFreezingWasTaken()													// Метод вызванный событием "Заморозка была взята"
	{
		gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[3];	// Делаем глаза этого призрака цветом указывающим что он в режиме заморозки
		gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[3];	// Делаем этого призрака цветом указывающим что он в режиме заморозки
		SpeedModifier = (Speed/100*GM.GameLevel);								// Устанавливаем модификатор скорости равным целой скорости этого приведения для этого уровня
	}


	void MethodGhostReturnNormalSpeed()											// Этот метод вызываеться событием "Возвратить приведениям нормальную скорость"
	{
		gameObject.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = GM.GhostsMats[0];	// Делаем глаза этого призрака нормального цвета указывающим что призрак вернул нормальную скорость
		gameObject.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().material = Original;			// Делаем этого призрака цветом указывающим что он вернул себе нормальную скорость
		SpeedModifier = 0;															// Устанавливаем модификатор скорости на 0
	}




	void MethodKillPacMan()															// Метод вызванный событием "Убить пекмена"	
	{
		GhostMode = GhostMove.GhostM.Expectation;								// Ставим всем приведениям режим ожидания
		DestinationPoint = transform.position;									// Ставим точку назначения призраку на тоже место где он стоит
		StartCoroutine(IEnumKillPacMan());										// Вызываем корутину "IEnumKillPacMan"
	}
	IEnumerator IEnumKillPacMan()													// Этот метод-корутина продолжение метода "MethodKillPacMan"
	{
		yield return new WaitForSeconds(2);										// Ждём 2 секунды	
		transform.position = StartPosition;										// Ставим призрака на его стартовую позицию
		transform.rotation = Quaternion.Euler (0, 0, 0);					// Ставим начальное вращение приведения
		DestinationPoint = transform.position;									// Ставим точку назначения призрака на тоже место где он стоит
		DelayTimer = Delay;															// Ставим задержку до выхода из дома установленную для этого приведения
		if(GM.PacmanLifes >= 0) 													// Если количество жизней пекмена больше ноля
		{
			if(GhostName != GhostN.Hunter)										// Если имя призрака не охотник
				Home = true;															// Указываем что призрак в этом цикле находиться дома
		}
		else 																				// Иначе если у пекмена меньше одной жизни
		{
			DelayTimer = 0;															// Ставим всем приведениям задержку 0 секунд для начала следующей игры
		}
	}


	void MethodAllDotsAreCollected()												// Этот метод запускает корутину завершения уровня
	{
		GhostMode = GhostM.Expectation;											// Включаем режим ожидания
		DestinationPoint = transform.position;									// Ставим точку назначения призрака на тоже место где он стоит		
		if(GhostName != GhostN.Hunter)											// Если имя этого призрака не охотник
		{
			DelayTimer = Delay;														// Ставим задержку до выхода из дома установленную для этого приведения
			Home = true;																// И указываем ему что он находиться дома
		}
		if(GM.GameLevel < 10)														// Если это был не последний уровень
		{
			StartCoroutine(IEnumAllDotsAreCollected());						// Вызываем Метод способный отсчитывать время	
		}
	}
	IEnumerator IEnumAllDotsAreCollected()
	{
		yield return new WaitForSeconds(1.5f);									// Ждём
		transform.position = StartPosition;										// Ставим призрака на его стартовую позицию
		DestinationPoint = transform.position;									// Ставим точку назначения призрака на тоже место где он стоит		
		transform.rotation = Quaternion.Euler(Vector3.zero);				// Поворачиваем его лицом вперёд
	}
	void MethodNewGame()																// Этот метод вызываеться событием "Новая игра"
	{
		if(GhostName != GhostN.Hunter)											// Если этот призрак не охотник
			Home = true;																// Указываем ему состояние что он дома
	}
	void MethodEndGame()																// Метод вызываемый событием "Конец игры"
	{
		StartCoroutine(EndGamePlus());											// Вызываем Метод-Корутину
	}
	IEnumerator EndGamePlus()														// Этот метод продолжение метода "Конец игры" но способный отсчитывать время
	{
		yield return new WaitForSeconds(4);										// Ждём
		transform.position = StartPosition;										// Ставим призрака на его стартовую позицию
		DestinationPoint = transform.position;									// Ставим точку назначения призрака на тоже место где он стоит		
		transform.rotation = Quaternion.Euler(Vector3.zero);				// Поворачиваем его лицом вперёд
		if(GhostName != GhostN.Hunter)											// Если имя этого призрака не охотник
		{
			DelayTimer = Delay;														// Ставим задержку до выхода из дома установленную для этого приведения
			Home = true;																// И указываем ему что он находиться дома
		}
	}


	void OnDisable()
	{
		GameManager.StartGame -= MethodStartGame;										// Отписываем метод "MethodStartGame" от события "StartGame" 
		GameManager.ToPursuit -= MethodToPursuit;										// Отписываем метод "MethodToPursuit" от cобытия "ToPursuit" 
		GameManager.ToRunUp -= MethodToRunUp;											// Отписываем метод "MethodToRunUp" от cобытия "ToRunUp" 
		GameManager.CherryWasTaken -= MethodCherryWasTaken;						// Отписываем метод "MethodCherryWasTaken" от события "CherryWasTaken"
		GameManager.DecelerationWasTaken -= MethodDecelerationWasTaken;		// Отписываем метод "MethodDecelerationWasTaken" от события "DecelerationWasTaken"
		GameManager.FreezingWasTaken -= MethodFreezingWasTaken;					// Отпписываем метод "MethodFreezingWasTaken" от события "FreezingWasTaken"
		GameManager.GhostReturnNormalSpeed -= MethodGhostReturnNormalSpeed;	// Отписываем метод "MethodGhostReturnNormalSpeed" от события "GhostReturnNormalSpeed"
		GameManager.PacManNotDanger -= MethodPacManNotDanger;						// Отписываем метод "PacManNotDanger" от события "MethodPacManNotDanger"
		GameManager.KillPacMan -= MethodKillPacMan;									// Отписываем метод "MethodKillPacMan" от события "KillPacMan"
		GameManager.AllDotsAreCollected -= MethodAllDotsAreCollected;			// Отписываем метод "MethodAllDotsAreCollected" от события "Все точки собранны" 
		GameManager.NewGame -= MethodNewGame;											// Отписываем метод "MethodNewGame" от события "NewGame"
		GameManager.EndGame -= MethodEndGame;											// Отписываем метод "MethodEndGame" от события "EndGame"
	}
}
